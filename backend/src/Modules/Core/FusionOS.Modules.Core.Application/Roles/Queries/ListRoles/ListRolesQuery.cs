using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Roles.Contracts;

namespace FusionOS.Modules.Core.Application.Roles.Queries.ListRoles;

/// <summary>
/// Read-gated (2026-07-14 sprint audit, Phase H2) — requires "core.role.manage".
/// Search added in Phase M5 (2026-07-15 — Search completion): matches on Name.
/// </summary>
public sealed record ListRolesQuery(Guid CompanyId, string? Search = null) : IQuery<IReadOnlyList<RoleDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.role.manage" };
}
