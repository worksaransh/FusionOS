using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;
using FusionOS.Modules.Finance.Domain.IntercompanyEliminationRules;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class IntercompanyEliminationRuleRepository : IIntercompanyEliminationRuleRepository
{
    private readonly FinanceDbContext _context;

    public IntercompanyEliminationRuleRepository(FinanceDbContext context) => _context = context;

    public Task<IntercompanyEliminationRule?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.IntercompanyEliminationRules.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == id, cancellationToken);

    public async Task AddAsync(IntercompanyEliminationRule rule, CancellationToken cancellationToken = default) =>
        await _context.IntercompanyEliminationRules.AddAsync(rule, cancellationToken);

    public async Task<IReadOnlyList<IntercompanyEliminationRule>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.IntercompanyEliminationRules
            .Where(r => r.CompanyId == companyId)
            .OrderBy(r => r.ReceivableAccountCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.IntercompanyEliminationRules.CountAsync(r => r.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<IntercompanyEliminationRule>> ListActiveAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        await _context.IntercompanyEliminationRules
            .Where(r => r.CompanyId == companyId && r.IsActive)
            .OrderBy(r => r.ReceivableAccountCode)
            .ToListAsync(cancellationToken);
}
