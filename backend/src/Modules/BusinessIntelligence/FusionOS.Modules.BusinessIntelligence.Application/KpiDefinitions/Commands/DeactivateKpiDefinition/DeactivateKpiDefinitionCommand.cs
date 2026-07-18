using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.DeactivateKpiDefinition;

public sealed record DeactivateKpiDefinitionCommand(Guid CompanyId, Guid KpiDefinitionId)
    : ICommand<KpiDefinitionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "bi.kpi-definition.deactivate" };
    public string EntityType => nameof(Domain.KpiDefinitions.KpiDefinition);
    public Guid EntityId => KpiDefinitionId;
    public string Action => "Deactivated";
}
