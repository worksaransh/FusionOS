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
/// requests themselves already carry that history. Spare parts tracking is
/// explicitly out of scope for this slice — a follow-up once there is a
/// concrete need to consume/track parts against a request.
///
/// AssignedTechnicianUserId (added alongside MaintenanceSchedule) is an opaque
/// reference into Core's own User — never existence-validated here, same
/// "opaque cross-module reference" convention as PickList.AssignedToUserId
/// (Warehouse). ActualDowntimeMinutes is recorded once, at Complete time —
/// there is no separate "downtime tracking" aggregate for this slice.
/// </summary>
public sealed class MaintenanceRequest : TenantAggregateRoot
{
    public Guid AssetId { get; private set; }
    public MaintenanceRequestType Type { get; private set; }
    public string Description { get; private set; } = default!;
    public MaintenanceRequestStatus Status { get; private set; }
    public DateTimeOffset ReportedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public Guid? AssignedTechnicianUserId { get; private set; }
    public int? ActualDowntimeMinutes { get; private set; }

    private MaintenanceRequest() { }

    public static MaintenanceRequest Create(Guid companyId, Guid assetId, MaintenanceRequestType type, string description)
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

    /// <summary>Resolves the request. Requires it to be InProgress — a request cannot be completed before someone started the work. ActualDowntimeMinutes is optional — not every breakdown causes downtime worth recording (e.g. a redundant unit kept the line running).</summary>
    public void Complete(string? resolutionNotes, int? actualDowntimeMinutes = null)
    {
        if (Status != MaintenanceRequestStatus.InProgress)
            throw new InvalidOperationException($"Only an InProgress maintenance request can be completed (current status: {Status}).");
        if (actualDowntimeMinutes is < 0)
            throw new ArgumentException("Actual downtime minutes cannot be negative.", nameof(actualDowntimeMinutes));

        Status = MaintenanceRequestStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ResolutionNotes = string.IsNullOrWhiteSpace(resolutionNotes) ? null : resolutionNotes.Trim();
        ActualDowntimeMinutes = actualDowntimeMinutes;
        Raise(new MaintenanceRequestCompleted(Id, CompanyId, AssetId, Type.ToString()));
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
