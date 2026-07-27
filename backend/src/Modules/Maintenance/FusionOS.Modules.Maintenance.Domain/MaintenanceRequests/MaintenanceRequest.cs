using FusionOS.SharedKernel;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests.Events;

namespace FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;

/// <summary>
/// Phase 5 — Maintenance, first slice: a preventive or breakdown maintenance
/// request against an Asset, Open → InProgress → Completed. AssetId is a
/// real, same-module foreign key (Asset lives in this module), validated by
/// the command handler via IAssetRepository — same convention as
/// CreateBudgetLine validating AccountId in Finance. Completed requests
/// against one Asset, listed together, are this slice's "maintenance
/// history" — a separate history/report aggregate is not needed since the
/// requests themselves already carry that history. Spare parts tracking was
/// explicitly out of scope for this slice originally — see the follow-up
/// note below for why/how that changed.
///
/// AssignedTechnicianUserId (added alongside MaintenanceSchedule) is an opaque
/// reference into Core's own User — never existence-validated here, same
/// "opaque cross-module reference" convention as PickList.AssignedToUserId
/// (Warehouse). ActualDowntimeMinutes is recorded once, at Complete time —
/// there is no separate "downtime tracking" aggregate for this slice.
///
/// Spare parts tracking (follow-up, 2026-07-23): SparePartUsage is an owned
/// child collection, recorded only while InProgress, same "record on the
/// parent, let another module react to the raised event" shape as
/// Manufacturing's WorkOrderComponent — RecordSparePartUsage does not itself
/// touch the Inventory ledger, it only raises SparePartsConsumedForMaintenance
/// for Inventory's own consumer to apply. LaborHours/LaborRatePerHour are
/// optional, caller-supplied figures recorded at Complete time; TotalCost is
/// computed on read from labor + the sum of spare part usage, never
/// persisted, since everything it sums is already durable. Posting this cost
/// to Finance's GL is an explicit non-goal of this slice — a real
/// "maintenance cost → Finance" integration is its own separately-scoped
/// follow-up, same as this doc comment's earlier "spare parts is out of
/// scope" note used to be.
///
/// Checklist snapshot (2026-07-23 follow-up): when CreateMaintenanceRequestCommand is
/// given an optional ChecklistTemplateId, the handler loads that
/// ChecklistTemplates.ChecklistTemplate and passes its step descriptions in here, where
/// they are copied by value onto MaintenanceRequestChecklistItem — never a live reference
/// to the template. A later edit to the template therefore never changes what an
/// already-created request's checklist says, same snapshot discipline as
/// Manufacturing's WorkOrderComponent. Purely a convenience — a request created without a
/// template simply has no checklist items.
/// </summary>
public sealed class MaintenanceRequest : TenantAggregateRoot
{
    private readonly List<SparePartUsage> _sparePartUsages = new();
    private readonly List<MaintenanceRequestChecklistItem> _checklistItems = new();

    public Guid AssetId { get; private set; }
    public MaintenanceRequestType Type { get; private set; }
    public string Description { get; private set; } = default!;
    public MaintenanceRequestStatus Status { get; private set; }
    public DateTimeOffset ReportedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public Guid? AssignedTechnicianUserId { get; private set; }
    public int? ActualDowntimeMinutes { get; private set; }
    public decimal? LaborHours { get; private set; }
    public decimal? LaborRatePerHour { get; private set; }
    public IReadOnlyCollection<SparePartUsage> SparePartUsages => _sparePartUsages;
    public IReadOnlyCollection<MaintenanceRequestChecklistItem> ChecklistItems => _checklistItems;

    /// <summary>(LaborHours × LaborRatePerHour, 0 if either is unset) + the sum of each spare part usage's QuantityUsed × UnitCost. Computed on read, not persisted.</summary>
    public decimal TotalCost =>
        (LaborHours ?? 0m) * (LaborRatePerHour ?? 0m) + _sparePartUsages.Sum(p => p.QuantityUsed * p.UnitCost);

    private MaintenanceRequest() { }

    public static MaintenanceRequest Create(Guid companyId, Guid assetId, MaintenanceRequestType type, string description, IReadOnlyCollection<string>? checklistItemDescriptions = null)
    {
        if (assetId == Guid.Empty)
            throw new ArgumentException("Asset id is required.", nameof(assetId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A description of the maintenance need is required.", nameof(description));

        var request = new MaintenanceRequest
        {
            CompanyId = companyId,
            AssetId = assetId,
            Type = type,
            Description = description.Trim(),
            Status = MaintenanceRequestStatus.Open,
            ReportedAt = DateTimeOffset.UtcNow,
        };

        if (checklistItemDescriptions is not null)
        {
            foreach (var step in checklistItemDescriptions)
                request._checklistItems.Add(MaintenanceRequestChecklistItem.Create(step));
        }

        request.Raise(new MaintenanceRequestCreated(request.Id, companyId, assetId, type.ToString()));
        return request;
    }

    /// <summary>Marks work as underway. Requires the request to still be Open — same "one clear starting state" discipline as WorkOrder.Release.</summary>
    public void Start()
    {
        if (Status != MaintenanceRequestStatus.Open)
            throw new InvalidOperationException($"Only an Open maintenance request can be started (current status: {Status}).");

        Status = MaintenanceRequestStatus.InProgress;
    }

    /// <summary>Resolves the request. Requires it to be InProgress — a request cannot be completed before someone started the work. ActualDowntimeMinutes is optional — not every breakdown causes downtime worth recording (e.g. a redundant unit kept the line running). LaborHours/LaborRatePerHour are likewise optional cost-tracking inputs (2026-07-23 follow-up) — recorded once, at Complete time, same as ActualDowntimeMinutes.</summary>
    public void Complete(string? resolutionNotes, int? actualDowntimeMinutes = null, decimal? laborHours = null, decimal? laborRatePerHour = null)
    {
        if (Status != MaintenanceRequestStatus.InProgress)
            throw new InvalidOperationException($"Only an InProgress maintenance request can be completed (current status: {Status}).");
        if (actualDowntimeMinutes is < 0)
            throw new ArgumentException("Actual downtime minutes cannot be negative.", nameof(actualDowntimeMinutes));
        if (laborHours is < 0)
            throw new ArgumentException("Labor hours cannot be negative.", nameof(laborHours));
        if (laborRatePerHour is < 0)
            throw new ArgumentException("Labor rate per hour cannot be negative.", nameof(laborRatePerHour));

        Status = MaintenanceRequestStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ResolutionNotes = string.IsNullOrWhiteSpace(resolutionNotes) ? null : resolutionNotes.Trim();
        ActualDowntimeMinutes = actualDowntimeMinutes;
        LaborHours = laborHours;
        LaborRatePerHour = laborRatePerHour;
        Raise(new MaintenanceRequestCompleted(Id, CompanyId, AssetId, Type.ToString()));
    }

    /// <summary>
    /// Records a spare part consumed against this request. Only permitted while
    /// InProgress — same "work must actually be underway" discipline as the request's
    /// own Start/Complete transitions; recording parts before work starts or after it's
    /// resolved would not correspond to a real consumption event. Raises
    /// SparePartsConsumedForMaintenance so Inventory's own consumer can post the real
    /// Stock Ledger movement — this method itself never touches Inventory.
    /// </summary>
    public void RecordSparePartUsage(Guid productId, Guid warehouseId, decimal quantityUsed, decimal unitCost)
    {
        if (Status != MaintenanceRequestStatus.InProgress)
            throw new InvalidOperationException($"Spare parts can only be recorded against an InProgress maintenance request (current status: {Status}).");

        var usage = SparePartUsage.Create(productId, warehouseId, quantityUsed, unitCost);
        _sparePartUsages.Add(usage);

        Raise(new SparePartsConsumedForMaintenance(Id, CompanyId, productId, warehouseId, quantityUsed));
    }

    /// <summary>
    /// Marks one snapshotted checklist step as done. Reuses maintenance.request.start as its
    /// permission (see CompleteMaintenanceRequestChecklistItemCommand's doc comment for why) —
    /// ticking off a step is part of doing the work, the same phase as Start, not the
    /// terminal Complete transition. Allowed any time before the request itself is
    /// Completed — same "not after the terminal state" rule as AssignTechnician.
    /// </summary>
    public void CompleteChecklistItem(Guid itemId)
    {
        if (Status == MaintenanceRequestStatus.Completed)
            throw new InvalidOperationException("This maintenance request is already completed — its checklist can no longer be updated.");

        var item = _checklistItems.FirstOrDefault(i => i.Id == itemId)
            ?? throw new ArgumentException($"Checklist item '{itemId}' does not exist on this maintenance request.", nameof(itemId));

        item.Complete();
    }

    /// <summary>
    /// Assigns (or reassigns) the technician responsible for this request. Allowed any time
    /// before Completed — a mid-repair reassignment (handing off to another technician) is a
    /// normal operation, not an error — same "assignable any time before the terminal state"
    /// rule as PickList.AssignTo (Warehouse). Does not raise its own domain event, matching
    /// that same precedent.
    /// </summary>
    public void AssignTechnician(Guid technicianUserId)
    {
        if (technicianUserId == Guid.Empty)
            throw new ArgumentException("Technician user id is required.", nameof(technicianUserId));
        if (Status == MaintenanceRequestStatus.Completed)
            throw new InvalidOperationException("This maintenance request is already completed — it cannot be reassigned.");

        AssignedTechnicianUserId = technicianUserId;
    }
}
