using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Queries.ListKpiDefinitions;

public sealed record ListKpiDefinitionsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<KpiDefinitionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "bi.kpi-definition.read" };
}
