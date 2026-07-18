using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Queries.GetKpiDefinitionById;

public sealed record GetKpiDefinitionByIdQuery(Guid CompanyId, Guid KpiDefinitionId) : IQuery<KpiDefinitionDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "bi.kpi-definition.read" };
}
