using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Domain.Budgets;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class BudgetRepository : IBudgetRepository
{
    private readonly FinanceDbContext _context;

    public BudgetRepository(FinanceDbContext context) => _context = context;

    public Task<Budget?> GetByIdAsync(Guid companyId, Guid budgetId, CancellationToken cancellationToken = default) =>
        _context.Budgets.FirstOrDefaultAsync(b => b.CompanyId == companyId && b.Id == budgetId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid budgetId, CancellationToken cancellationToken = default) =>
        _context.Budgets.AnyAsync(b => b.CompanyId == companyId && b.Id == budgetId, cancellationToken);

    public async Task AddAsync(Budget budget, CancellationToken cancellationToken = default) =>
        await _context.Budgets.AddAsync(budget, cancellationToken);

    public async Task<IReadOnlyList<Budget>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderByDescending(b => b.PeriodStart)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Budget> Filtered(Guid companyId, string? search)
    {
        var query = _context.Budgets.Where(b => b.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(b => EF.Functions.ILike(b.Name, pattern));
        }
        return query;
    }
}
