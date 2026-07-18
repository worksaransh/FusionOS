using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Roles.Contracts;

namespace FusionOS.Modules.Core.Application.Roles.Commands.CreateRole;

/// <summary>
/// Creates a company-scoped custom role with no permissions granted yet — use
/// SetRolePermissionsCommand afterward to grant it any (2026-07-14 sprint audit,
/// Phase H2). Requires "core.role.manage" (07_SECURITY.md §2), unlike the
/// auto-created "Owner" role every company gets for free at bootstrap.
/// </summary>
public sealed record CreateRoleCommand(Guid CompanyId, string Name)
    : ICommand<RoleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.role.manage" };
    public string EntityType => "Role";
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
