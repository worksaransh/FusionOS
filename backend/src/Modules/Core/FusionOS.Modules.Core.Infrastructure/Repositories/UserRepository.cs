using FusionOS.Modules.Core.Application.Auth;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Domain.Identity;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly CoreDbContext _context;

    public UserRepository(CoreDbContext context) => _context = context;

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct) => await _context.Users.AddAsync(user, ct);

    public async Task<IReadOnlyCollection<(Guid CompanyId, Guid? BranchId, Guid RoleId)>> GetCompanyRolesAsync(Guid userId, CancellationToken ct) =>
        await _context.UserCompanyRoles
            .Where(ucr => ucr.UserId == userId)
            .Select(ucr => new ValueTuple<Guid, Guid?, Guid>(ucr.CompanyId, ucr.BranchId, ucr.RoleId))
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<string>> GetRolePermissionCodesAsync(Guid roleId, CancellationToken ct) =>
        await (from rp in _context.RolePermissions
               join p in _context.Permissions on rp.PermissionId equals p.Id
               where rp.RoleId == roleId
               select p.Code)
            .ToListAsync(ct);

    public async Task<Guid> GetOrCreateCompanyOwnerRoleAsync(Guid companyId, CancellationToken ct)
    {
        await EnsurePermissionsSeededAsync(ct);

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Name == "Owner", ct);
        if (role is null)
        {
            role = Role.CreateCompanyRole(companyId, "Owner");
            await _context.Roles.AddAsync(role, ct);
            await _context.SaveChangesAsync(ct);
        }

        var allPermissionIds = await _context.Permissions.Select(p => p.Id).ToListAsync(ct);
        var alreadyGranted = await _context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct);

        var missing = allPermissionIds.Except(alreadyGranted);
        foreach (var permissionId in missing)
        {
            await _context.RolePermissions.AddAsync(RolePermission.Create(role.Id, permissionId), ct);
        }

        return role.Id;
    }

    public async Task LinkUserToCompanyAsync(Guid userId, Guid companyId, Guid roleId, Guid? branchId, CancellationToken ct)
    {
        var alreadyLinked = await _context.UserCompanyRoles
            .AnyAsync(ucr => ucr.UserId == userId && ucr.CompanyId == companyId && ucr.RoleId == roleId, ct);
        if (!alreadyLinked)
            await _context.UserCompanyRoles.AddAsync(UserCompanyRole.Create(userId, companyId, roleId, branchId), ct);
    }

    public Task<bool> CompanyHasAnyUsersAsync(Guid companyId, CancellationToken ct) =>
        _context.UserCompanyRoles.AnyAsync(ucr => ucr.CompanyId == companyId, ct);

    private async Task EnsurePermissionsSeededAsync(CancellationToken ct)
    {
        var existingCodes = await _context.Permissions.Select(p => p.Code).ToListAsync(ct);
        foreach (var (module, code, description) in PermissionCatalog.All)
        {
            if (!existingCodes.Contains(code))
                await _context.Permissions.AddAsync(Permission.Create(module, code, description), ct);
        }
    }
}
