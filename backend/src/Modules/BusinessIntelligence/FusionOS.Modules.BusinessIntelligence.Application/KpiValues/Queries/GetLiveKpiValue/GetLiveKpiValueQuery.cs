using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Queries.GetLiveKpiValue;

/// <summary>Phase 2.3 — computes a KPI's current value live instead of requiring a manually-recorded bi.kpi-snapshot.record.</summary>
public sealed record GetLiveKpiValueQuery(Guid CompanyId, Guid KpiDefinitionId) : IQuery<LiveKpiValueDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "bi.kpi-value.read" };
}
