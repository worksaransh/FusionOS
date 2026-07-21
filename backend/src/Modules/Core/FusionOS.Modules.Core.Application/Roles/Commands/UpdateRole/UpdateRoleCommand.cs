using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Roles.Contracts;

namespace FusionOS.Modules.Core.Application.Roles.Commands.UpdateRole;

/// <summary>
/// Renames a company-custom role (Role.Rename() already existed on the domain
/// entity with no command ever calling it before this). System roles cannot
/// be renamed - Role.Rename() itself enforces that and the handler surfaces
/// it as the same ValidationException shape as CreateRoleCommandHandler's
/// duplicate-name check, not a 500.
/// </summary>
public sealed record UpdateRoleCommand(Guid CompanyId, Guid RoleId, string Name)
    : ICommand<RoleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.role.manage" };
    public string EntityType => "Role";
    public Guid EntityId => RoleId;
    public string Action => "Renamed";
}
