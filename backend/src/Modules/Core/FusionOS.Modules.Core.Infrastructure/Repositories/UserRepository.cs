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

    // Phase H3 (2026-07-14 sprint audit): every registration into a company that
    // already has at least one user used to silently receive the same
    // all-permissions "Owner" role as the company's genuine first (bootstrap)
    // user - there was no way to invite a teammate at a lesser privilege. This
    // is the safe landing spot for that case: a company-scoped "Member" role
    // that is created once, with zero permissions granted, ever. An existing
    // Owner promotes a Member afterward via SetRolePermissionsCommand (grant it
    // permissions directly) or AssignUserRoleCommand (move the user onto a
    // different, already-privileged role) - both surfaced on RolesPage.
    public async Task<Guid> GetOrCreateDefaultMemberRoleAsync(Guid companyId, CancellationToken ct)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Name == "Member", ct);
        if (role is null)
        {
            role = Role.CreateCompanyRole(companyId, "Member");
            await _context.Roles.AddAsync(role, ct);
            await _context.SaveChangesAsync(ct);
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

    // --- RBAC administration (2026-07-14 sprint audit, Phase H2) --------------------

    public Task<bool> RoleNameExistsAsync(Guid companyId, string name, Guid? excludeRoleId, CancellationToken ct) =>
        _context.Roles.AnyAsync(r => r.CompanyId == companyId && r.Name.ToLower() == name.Trim().ToLower()
            && (excludeRoleId == null || r.Id != excludeRoleId), ct);

    public async Task AddRoleAsync(Role role, CancellationToken ct) => await _context.Roles.AddAsync(role, ct);

    public Task<Role?> GetRoleByIdAsync(Guid roleId, Guid companyId, CancellationToken ct) =>
        _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.CompanyId == companyId, ct);

    public async Task<IReadOnlyList<Role>> ListRolesByCompanyAsync(Guid companyId, string? search, CancellationToken ct)
    {
        var query = _context.Roles.Where(r => r.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(r => EF.Functions.ILike(r.Name, pattern));
        }

        return await query.OrderBy(r => r.Name).ToListAsync(ct);
    }

    public async Task SetRolePermissionsAsync(Guid roleId, IReadOnlyCollection<string> permissionCodes, CancellationToken ct)
    {
        await EnsurePermissionsSeededAsync(ct);

        var wantedPermissionIds = await _context.Permissions
            .Where(p => permissionCodes.Contains(p.Code))
            .Select(p => p.Id)
            .ToListAsync(ct);

        var currentGrants = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(ct);
        var currentPermissionIds = currentGrants.Select(rp => rp.PermissionId).ToHashSet();

        var toAdd = wantedPermissionIds.Where(id => !currentPermissionIds.Contains(id));
        foreach (var permissionId in toAdd)
        {
            await _context.RolePermissions.AddAsync(RolePermission.Create(roleId, permissionId), ct);
        }

        var toRemove = currentGrants.Where(rp => !wantedPermissionIds.Contains(rp.PermissionId));
        _context.RolePermissions.RemoveRange(toRemove);
    }

    public async Task<IReadOnlyList<(Guid UserId, string Email, string FullName, Guid RoleId, string RoleName, bool IsActive)>> ListCompanyUsersAsync(Guid companyId, string? search, CancellationToken ct)
    {
        var query =
            from ucr in _context.UserCompanyRoles
            join u in _context.Users on ucr.UserId equals u.Id
            join r in _context.Roles on ucr.RoleId equals r.Id
            where ucr.CompanyId == companyId
            select new { u.Id, u.Email, u.FullName, RoleId = r.Id, RoleName = r.Name, u.IsActive };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Email, pattern) || EF.Functions.ILike(x.FullName, pattern));
        }

        var rows = await query.OrderBy(x => x.Email).ToListAsync(ct);
        return rows.Select(x => new ValueTuple<Guid, string, string, Guid, string, bool>(x.Id, x.Email, x.FullName, x.RoleId, x.RoleName, x.IsActive)).ToList();
    }

    public async Task AssignUserRoleAsync(Guid userId, Guid companyId, Guid roleId, CancellationToken ct)
    {
        var existing = await _context.UserCompanyRoles
            .Where(ucr => ucr.UserId == userId && ucr.CompanyId == companyId)
            .ToListAsync(ct);

        if (existing.Count == 1 && existing[0].RoleId == roleId)
            return;

        if (existing.Count > 0)
            _context.UserCompanyRoles.RemoveRange(existing);

        await _context.UserCompanyRoles.AddAsync(UserCompanyRole.Create(userId, companyId, roleId), ct);
    }
}
