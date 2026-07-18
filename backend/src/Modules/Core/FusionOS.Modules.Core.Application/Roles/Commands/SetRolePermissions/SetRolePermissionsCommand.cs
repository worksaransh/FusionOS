using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.Roles.Commands.SetRolePermissions;

/// <summary>
/// Replaces a role's entire permission set with exactly the given codes
/// (2026-07-14 sprint audit, Phase H2). Unknown codes are rejected by the
/// validator, not silently dropped.
/// </summary>
public sealed record SetRolePermissionsCommand(Guid CompanyId, Guid RoleId, IReadOnlyList<string> PermissionCodes)
    : ICommand<IReadOnlyList<string>>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.role.manage" };
    public string EntityType => "Role";
    public Guid EntityId => RoleId;
    public string Action => "PermissionsUpdated";
}
