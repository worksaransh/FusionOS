using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Branches.Contracts;

namespace FusionOS.Modules.Core.Application.Branches.Commands.UpdateBranch;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateCostCenterCommand/UpdateCompanyCommand.</summary>
public sealed record UpdateBranchCommand(Guid CompanyId, Guid BranchId, string Name, bool IsHeadOffice)
    : ICommand<BranchDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.branch.update" };
    public string EntityType => nameof(Domain.Organizations.Branch);
    public Guid EntityId => BranchId;
    public string Action => "Updated";
}
