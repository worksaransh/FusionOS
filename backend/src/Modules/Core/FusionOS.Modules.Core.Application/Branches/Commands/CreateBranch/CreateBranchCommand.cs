using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Branches.Contracts;

namespace FusionOS.Modules.Core.Application.Branches.Commands.CreateBranch;

public sealed record CreateBranchCommand(Guid CompanyId, string Name, string Code, bool IsHeadOffice = false)
    : ICommand<BranchDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.branch.create" };
    public string EntityType => nameof(Domain.Organizations.Branch);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
