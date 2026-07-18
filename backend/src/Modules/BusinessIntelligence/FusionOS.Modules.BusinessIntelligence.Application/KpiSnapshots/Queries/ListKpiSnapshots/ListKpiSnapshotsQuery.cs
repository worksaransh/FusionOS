using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Queries.ListKpiSnapshots;

/// <summary>KpiDefinitionId is optional — omitted, this lists every snapshot for the company; supplied, it scopes to one KPI's own time series (the "chart" a dashboard renders, 05_MODULE_ROADMAP.md's "Charts" line item).</summary>
public sealed record ListKpiSnapshotsQuery(Guid CompanyId, Guid? KpiDefinitionId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<KpiSnapshotDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "bi.kpi-snapshot.read" };
}
