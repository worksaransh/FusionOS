using FusionOS.SharedKernel;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules.Events;

namespace FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;

/// <summary>
/// A preventive-maintenance recurrence plan against an Asset — "the machine gets
/// serviced every Quarter, next due on this date" — distinct from
/// MaintenanceRequest, which is a single, already-reported (preventive or
/// breakdown) unit of work. This slice does not auto-generate a
/// MaintenanceRequest when NextDueDate arrives; that wiring (a scheduled job or
/// a "generate request from due schedule" command) is a natural, separately-
/// scoped follow-up once this aggregate exists to reference, same restraint as
/// MaintenanceRequest's own doc comment about spare-parts tracking.
///
/// AssetId is a real, same-module foreign key (Asset lives in this module),
/// validated by the command handler via IAssetRepository — same convention as
/// MaintenanceRequest.AssetId / CreateBudgetLine's AccountId validation.
/// </summary>
public sealed class MaintenanceSchedule : TenantAggregateRoot
{
    public Guid AssetId { get; private set; }
    public string Description { get; private set; } = default!;
    public MaintenanceScheduleFrequency Frequency { get; private set; }
    public DateTimeOffset NextDueDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    private MaintenanceSchedule() { }

    public static MaintenanceSchedule Create(Guid companyId, Guid assetId, MaintenanceScheduleFrequency frequency, string description, DateTimeOffset nextDueDate)
    {
        if (assetId == Guid.Empty)
            throw new ArgumentException("Asset id is required.", nameof(assetId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A description of the scheduled maintenance is required.", nameof(description));

        var schedule = new MaintenanceSchedule
        {
            CompanyId = companyId,
            AssetId = assetId,
            Frequency = frequency,
            Description = description.Trim(),
            NextDueDate = nextDueDate,
            IsActive = true,
        };

        schedule.Raise(new MaintenanceScheduleCreated(schedule.Id, companyId, assetId, frequency.ToString()));
        return schedule;
    }

    /// <summary>Updates the mutable recurrence fields. AssetId is not editable — same "the reference is set at creation" convention as MaintenanceRequest.AssetId.</summary>
    public void UpdateDetails(MaintenanceScheduleFrequency frequency, string description, DateTimeOffset nextDueDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A description of the scheduled maintenance is required.", nameof(description));

        Frequency = frequency;
        Description = description.Trim();
        NextDueDate = nextDueDate;
    }

    public void Deactivate() => IsActive = false;
}
