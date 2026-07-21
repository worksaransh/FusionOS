using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Branches.Contracts;

namespace FusionOS.Modules.Core.Application.Branches.Commands.DeactivateBranch;

/// <summary>Soft-deactivate only — never a real delete (a branch may already be referenced by historical records), same convention as DeactivateCostCenterCommand.</summary>
public sealed record DeactivateBranchCommand(Guid CompanyId, Guid BranchId)
    : ICommand<BranchDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.branch.deactivate" };
    public string EntityType => nameof(Domain.Organizations.Branch);
    public Guid EntityId => BranchId;
    public string Action => "Deactivated";
}
