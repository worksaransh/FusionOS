using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using FusionOS.Modules.Finance.Domain.BudgetLines;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class BudgetLineRepository : IBudgetLineRepository
{
    private readonly FinanceDbContext _context;

    public BudgetLineRepository(FinanceDbContext context) => _context = context;

    public Task<BudgetLine?> GetByIdAsync(Guid companyId, Guid budgetLineId, CancellationToken cancellationToken = default) =>
        _context.BudgetLines.FirstOrDefaultAsync(l => l.CompanyId == companyId && l.Id == budgetLineId, cancellationToken);

    public async Task AddAsync(BudgetLine budgetLine, CancellationToken cancellationToken = default) =>
        await _context.BudgetLines.AddAsync(budgetLine, cancellationToken);

    public async Task<IReadOnlyList<BudgetLine>> ListByBudgetAsync(Guid companyId, Guid budgetId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, budgetId)
            .OrderBy(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountByBudgetAsync(Guid companyId, Guid budgetId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, budgetId).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<BudgetLine>> ListAllByBudgetAsync(Guid companyId, Guid budgetId, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, budgetId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

    private IQueryable<BudgetLine> Filtered(Guid companyId, Guid budgetId) =>
        _context.BudgetLines.Where(l => l.CompanyId == companyId && l.BudgetId == budgetId);
}
