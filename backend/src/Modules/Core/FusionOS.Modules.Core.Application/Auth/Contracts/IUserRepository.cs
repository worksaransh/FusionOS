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

    Task LinkUserToCompanyAsync(Guid userId, Guid companyId, Guid roleId, Guid? branchId, CancellationToken ct);

    Task<bool> CompanyHasAnyUsersAsync(Guid companyId, CancellationToken ct);
}
