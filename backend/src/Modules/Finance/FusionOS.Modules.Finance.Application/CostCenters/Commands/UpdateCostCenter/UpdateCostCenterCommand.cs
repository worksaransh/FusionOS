using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;

namespace FusionOS.Modules.Finance.Application.CostCenters.Commands.UpdateCostCenter;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateAccountCommand.</summary>
public sealed record UpdateCostCenterCommand(Guid CompanyId, Guid CostCenterId, string Name)
    : ICommand<CostCenterDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.cost-center.update" };
    public string EntityType => nameof(Domain.CostCenters.CostCenter);
    public Guid EntityId => CostCenterId;
    public string Action => "Updated";
}
