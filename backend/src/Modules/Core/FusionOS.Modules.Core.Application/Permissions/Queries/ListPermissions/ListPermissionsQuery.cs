using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Permissions.Contracts;

namespace FusionOS.Modules.Core.Application.Permissions.Queries.ListPermissions;

/// <summary>
/// The full permission catalog (2026-07-14 sprint audit, Phase H2) — global
/// reference data, not company-scoped, so this intentionally carries no
/// CompanyId (TenantIsolationBehavior passes requests with no CompanyId
/// property through untouched, same as CreateCompanyCommand/LoginCommand).
/// Still requires "core.role.manage" since knowing the exact catalog shape is
/// only useful to whoever is administering roles. Search added in Phase M5
/// (2026-07-15 — Search completion): matches on Module, Code, or Description,
/// filtered in-memory since PermissionCatalog.All is a static list, not a
/// database table.
/// </summary>
public sealed record ListPermissionsQuery(string? Search = null) : IQuery<IReadOnlyList<PermissionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.role.manage" };
}
