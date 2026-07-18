using FusionOS.SharedKernel;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiDefinitions.Events;

namespace FusionOS.Modules.BusinessIntelligence.Domain.KpiDefinitions;

/// <summary>
/// Phase 6 — Business Intelligence, first slice: the KPI catalog
/// (05_MODULE_ROADMAP.md's "KPIs" line item). Pure master data (Code/Name/
/// Unit/IsActive), same shape as Finance's CostCenter. Deliberately does not
/// compute or ingest values from other modules automatically — this codebase's
/// own governing principle (docs/MASTER_FUTURE_BUILD_PLAN.md §2) requires BI
/// to be a consumer of events/read-models, never a synchronous dependency of
/// a transactional module, and no such event-driven KPI pipeline exists yet.
/// KpiSnapshot (this same slice) is instead a manually-recorded point-in-time
/// value against a KpiDefinition — real, useful today, and the natural future
/// hook for an automated event-fed pipeline once one exists, without this
/// slice pretending to be that pipeline now.
/// </summary>
public sealed class KpiDefinition : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Unit { get; private set; }
    public bool IsActive { get; private set; } = true;

    private KpiDefinition() { }

    public static KpiDefinition Create(Guid companyId, string code, string name, string? unit)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("KPI code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("KPI name is required.", nameof(name));

        var kpi = new KpiDefinition
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
        };

        kpi.Raise(new KpiDefinitionCreated(kpi.Id, companyId, kpi.Code));
        return kpi;
    }

    public void Deactivate() => IsActive = false;
}
