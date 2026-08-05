using FusionOS.Modules.Core.Application.SodConflictRules.Contracts;
using FusionOS.Modules.Core.Domain.SodConflictRules;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class SodConflictRuleRepository : ISodConflictRuleRepository
{
    private readonly CoreDbContext _context;

    public SodConflictRuleRepository(CoreDbContext context) => _context = context;

    public Task<SodConflictRule?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.SodConflictRules.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == id, cancellationToken);

    public async Task AddAsync(SodConflictRule rule, CancellationToken cancellationToken = default) =>
        await _context.SodConflictRules.AddAsync(rule, cancellationToken);

    public async Task<IReadOnlyList<SodConflictRule>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.SodConflictRules
            .Where(r => r.CompanyId == companyId)
            .OrderBy(r => r.ConflictingPermissionA)
            .ThenBy(r => r.ConflictingPermissionB)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.SodConflictRules.CountAsync(r => r.CompanyId == companyId, cancellationToken);

    public Task<bool> ConflictExistsAsync(Guid companyId, string permissionA, string permissionB, CancellationToken cancellationToken = default)
    {
        var a = permissionA.Trim();
        var b = permissionB.Trim();
        return _context.SodConflictRules.AnyAsync(r =>
            r.CompanyId == companyId &&
            ((r.ConflictingPermissionA == a && r.ConflictingPermissionB == b) ||
             (r.ConflictingPermissionA == b && r.ConflictingPermissionB == a)),
            cancellationToken);
    }

    public async Task<bool> HasActiveConflictAsync(Guid companyId, string permissionA, string permissionB, CancellationToken cancellationToken = default)
    {
        var a = permissionA.Trim();
        var b = permissionB.Trim();
        var candidates = await _context.SodConflictRules
            .Where(r => r.CompanyId == companyId && r.IsActive &&
                ((r.ConflictingPermissionA == a && r.ConflictingPermissionB == b) ||
                 (r.ConflictingPermissionA == b && r.ConflictingPermissionB == a)))
            .AnyAsync(cancellationToken);
        return candidates;
    }
}
