using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.Users.Commands.AssignUserRole;

/// <summary>
/// Sets a user's role within this company to exactly this one role, replacing
/// whatever role they previously held here (2026-07-14 sprint audit, Phase H2).
/// This is the operation that makes the read/write permission gates added in
/// Phase H1/H2 mean something beyond the single auto-granted "Owner" role.
/// </summary>
public sealed record AssignUserRoleCommand(Guid CompanyId, Guid UserId, Guid RoleId)
    : ICommand, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.role.manage" };
    public string EntityType => "UserCompanyRole";
    public Guid EntityId => UserId;
    public string Action => "RoleAssigned";
}
