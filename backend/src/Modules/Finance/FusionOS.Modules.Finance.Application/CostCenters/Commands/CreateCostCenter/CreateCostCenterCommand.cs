using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;

namespace FusionOS.Modules.Finance.Application.CostCenters.Commands.CreateCostCenter;

public sealed record CreateCostCenterCommand(Guid CompanyId, string Code, string Name)
    : ICommand<CostCenterDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.cost-center.create" };
    public string EntityType => nameof(Domain.CostCenters.CostCenter);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
