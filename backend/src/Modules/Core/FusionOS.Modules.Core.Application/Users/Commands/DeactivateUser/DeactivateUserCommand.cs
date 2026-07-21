using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.Users.Commands.DeactivateUser;

/// <summary>
/// Deactivates a user's account (User.Deactivate() already existed on the
/// domain entity - no command ever called it before this). CompanyId is
/// required so the caller can only act on a user who is actually a member of
/// their own company, mirroring AssignUserRoleCommand's membership check -
/// User itself is a global identity, not tenant-scoped, so without that check
/// any company admin could deactivate any user in the system.
/// </summary>
public sealed record DeactivateUserCommand(Guid CompanyId, Guid UserId)
    : ICommand, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.user.deactivate" };
    public string EntityType => "User";
    public Guid EntityId => UserId;
    public string Action => "Deactivated";
}
