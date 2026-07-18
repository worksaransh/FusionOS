using FusionOS.SharedKernel;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots.Events;

namespace FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots;

/// <summary>
/// Phase 6 — Business Intelligence, first slice: a manually-recorded,
/// point-in-time value against a KpiDefinition (05_MODULE_ROADMAP.md's
/// "Dashboards"/"KPIs" line items — the time series a dashboard chart
/// renders). KpiDefinitionId is a same-module FK, existence-validated in the
/// command handler (mirrors CreateBudgetLine/AccountId,
/// CreateMaintenanceRequest/AssetId, CreateLeaveRequest/EmployeeId).
/// Immutable once recorded — same "append-only, corrections are new entries"
/// reasoning as InventoryLedgerEntry, not something this slice needs an
/// approve/reject workflow for (unlike MaintenanceRequest/LeaveRequest,
/// which model a real human decision this doesn't).
/// </summary>
public sealed class KpiSnapshot : TenantAggregateRoot
{
    public Guid KpiDefinitionId { get; private set; }
    public decimal Value { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public string? Notes { get; private set; }

    private KpiSnapshot() { }

    public static KpiSnapshot Create(Guid companyId, Guid kpiDefinitionId, decimal value, string? notes)
    {
        if (kpiDefinitionId == Guid.Empty)
            throw new ArgumentException("KPI definition id is required.", nameof(kpiDefinitionId));

        var snapshot = new KpiSnapshot
        {
            CompanyId = companyId,
            KpiDefinitionId = kpiDefinitionId,
            Value = value,
            RecordedAt = DateTimeOffset.UtcNow,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        };

        snapshot.Raise(new KpiSnapshotRecorded(snapshot.Id, companyId, kpiDefinitionId, value));
        return snapshot;
    }
}
