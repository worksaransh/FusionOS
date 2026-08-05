using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.UpdateKpiDefinition;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateBranchCommand/UpdateCostCenterCommand/UpdateCompanyCommand.</summary>
public sealed record UpdateKpiDefinitionCommand(Guid CompanyId, Guid KpiDefinitionId, string Name, string? Unit)
    : ICommand<KpiDefinitionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "bi.kpi-definition.update" };
    public string EntityType => nameof(Domain.KpiDefinitions.KpiDefinition);
    public Guid EntityId => KpiDefinitionId;
    public string Action => "Updated";
}
