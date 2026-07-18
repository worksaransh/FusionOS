using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.CreateKpiDefinition;

public sealed record CreateKpiDefinitionCommand(Guid CompanyId, string Code, string Name, string? Unit)
    : ICommand<KpiDefinitionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "bi.kpi-definition.create" };
    public string EntityType => nameof(Domain.KpiDefinitions.KpiDefinition);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
