using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.Roles.Queries.GetRolePermissions;

/// <summary>Read-gated (2026-07-14 sprint audit, Phase H2) — requires "core.role.manage".</summary>
public sealed record GetRolePermissionsQuery(Guid CompanyId, Guid RoleId) : IQuery<IReadOnlyList<string>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.role.manage" };
}
