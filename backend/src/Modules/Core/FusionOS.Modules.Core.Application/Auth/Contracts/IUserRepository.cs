using FusionOS.Modules.Core.Domain.Identity;

namespace FusionOS.Modules.Core.Application.Auth.Contracts;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);

    /// <summary>All (companyId, roleId) pairs a user holds, across every company they belong to.</summary>
    Task<IReadOnlyCollection<(Guid CompanyId, Guid? BranchId, Guid RoleId)>> GetCompanyRolesAsync(Guid userId, CancellationToken ct);

    /// <summary>Permission codes granted to a role.</summary>
    Task<IReadOnlyCollection<string>> GetRolePermissionCodesAsync(Guid roleId, CancellationToken ct);

    /// <summary>Finds (or creates, with every known Permission attached) the company's bootstrap "Owner" role.</summary>
    Task<Guid> GetOrCreateCompanyOwnerRoleAsync(Guid companyId, CancellationToken ct);

    /// <summary>
    /// Finds (or creates, with zero permissions granted) the company's default
    /// "Member" role — the safe landing role for every registration after the
    /// company's bootstrap first user (Phase H3, 2026-07-14 sprint audit).
    /// An existing Owner promotes a Member afterward via SetRolePermissionsCommand
    /// or AssignUserRoleCommand (RolesPage).
    /// </summary>
    Task<Guid> GetOrCreateDefaultMemberRoleAsync(Guid companyId, CancellationToken ct);

    Task LinkUserToCompanyAsync(Guid userId, Guid companyId, Guid roleId, Guid? branchId, CancellationToken ct);

    Task<bool> CompanyHasAnyUsersAsync(Guid companyId, CancellationToken ct);

    // --- RBAC administration (2026-07-14 sprint audit, Phase H2) --------------------
    // Role/Permission persistence has lived inside this repository since
    // GetOrCreateCompanyOwnerRoleAsync was first written; these additions follow
    // that same convention rather than fragmenting Role/Permission access across
    // a second repository for what is, today, a single Core-module concern.

    /// <summary>True if a role with this name already exists for the company (case-insensitive).</summary>
    Task<bool> RoleNameExistsAsync(Guid companyId, string name, CancellationToken ct);

    Task AddRoleAsync(Role role, CancellationToken ct);

    Task<Role?> GetRoleByIdAsync(Guid roleId, Guid companyId, CancellationToken ct);

    /// <summary>Search (Phase M5, 2026-07-15) matches on Role.Name — optional, null/blank returns every role.</summary>
    Task<IReadOnlyList<Role>> ListRolesByCompanyAsync(Guid companyId, string? search, CancellationToken ct);

    /// <summary>
    /// Replaces every permission currently granted to the role with exactly the
    /// given set of permission codes (grants what's missing, revokes what's no
    /// longer listed). Unknown codes are silently ignored by the caller's own
    /// validation, not here - this method trusts its input.
    /// </summary>
    Task SetRolePermissionsAsync(Guid roleId, IReadOnlyCollection<string> permissionCodes, CancellationToken ct);

    /// <summary>
    /// Every user linked to this company, with the name of whichever role they
    /// currently hold. Search (Phase M5, 2026-07-15) matches on Email or FullName.
    /// </summary>
    Task<IReadOnlyList<(Guid UserId, string Email, string FullName, Guid RoleId, string RoleName)>> ListCompanyUsersAsync(Guid companyId, string? search, CancellationToken ct);

    /// <summary>
    /// Sets the user's role within this company to exactly this one role,
    /// replacing any role(s) they previously held here. Idempotent no-op if
    /// they already hold only this role.
    /// </summary>
    Task AssignUserRoleAsync(Guid userId, Guid companyId, Guid roleId, CancellationToken ct);
}
