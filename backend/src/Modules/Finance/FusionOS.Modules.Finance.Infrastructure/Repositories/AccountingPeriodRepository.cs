using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class AccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly FinanceDbContext _context;

    public AccountingPeriodRepository(FinanceDbContext context) => _context = context;

    public Task<AccountingPeriod?> GetByIdAsync(Guid companyId, Guid accountingPeriodId, CancellationToken cancellationToken = default) =>
        _context.AccountingPeriods.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Id == accountingPeriodId, cancellationToken);

    public async Task AddAsync(AccountingPeriod period, CancellationToken cancellationToken = default) =>
        await _context.AccountingPeriods.AddAsync(period, cancellationToken);

    public async Task<IReadOnlyList<AccountingPeriod>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.AccountingPeriods
            .Where(p => p.CompanyId == companyId)
            .OrderByDescending(p => p.PeriodStart)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.AccountingPeriods.CountAsync(p => p.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<AccountingPeriod>> ListAllAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        await _context.AccountingPeriods
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.PeriodStart)
            .ToListAsync(cancellationToken);

    public Task<AccountingPeriod?> FindClosedPeriodCoveringDateAsync(Guid companyId, DateTimeOffset date, CancellationToken cancellationToken = default) =>
        _context.AccountingPeriods.FirstOrDefaultAsync(
            p => p.CompanyId == companyId
                 && p.Status == AccountingPeriodStatus.Closed
                 && p.PeriodStart <= date
                 && p.PeriodEnd >= date,
            cancellationToken);
}
