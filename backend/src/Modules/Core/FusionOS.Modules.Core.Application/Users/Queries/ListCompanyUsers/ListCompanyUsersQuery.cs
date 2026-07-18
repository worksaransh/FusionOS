using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Users.Contracts;

namespace FusionOS.Modules.Core.Application.Users.Queries.ListCompanyUsers;

/// <summary>
/// Read-gated (2026-07-14 sprint audit, Phase H2) — requires "core.role.manage".
/// Search added in Phase M5 (2026-07-15 — Search completion): matches on Email or FullName.
/// </summary>
public sealed record ListCompanyUsersQuery(Guid CompanyId, string? Search = null) : IQuery<IReadOnlyList<CompanyUserDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.role.manage" };
}
