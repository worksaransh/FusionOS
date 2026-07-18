using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;

namespace FusionOS.Modules.Finance.Application.CostCenters.Commands.DeactivateCostCenter;

/// <summary>Soft-deactivate only — never a real delete (a cost center may already be referenced by historical postings once journal-line attachment is wired up).</summary>
public sealed record DeactivateCostCenterCommand(Guid CompanyId, Guid CostCenterId)
    : ICommand<CostCenterDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.cost-center.deactivate" };
    public string EntityType => nameof(Domain.CostCenters.CostCenter);
    public Guid EntityId => CostCenterId;
    public string Action => "Deactivated";
}
