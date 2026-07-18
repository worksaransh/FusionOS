using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Domain.Payables;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class ApLedgerRepository : IApLedgerRepository
{
    private readonly FinanceDbContext _context;

    public ApLedgerRepository(FinanceDbContext context) => _context = context;

    public async Task AddAsync(ApLedgerEntry entry, CancellationToken cancellationToken = default) =>
        await _context.ApLedgerEntries.AddAsync(entry, cancellationToken);

    public Task<decimal> SumAmountAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default) =>
        _context.ApLedgerEntries
            .Where(x => x.CompanyId == companyId && x.SupplierId == supplierId)
            .SumAsync(x => x.Amount, cancellationToken);

    public async Task<IReadOnlyList<ApLedgerEntry>> ListAsync(Guid companyId, Guid supplierId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.ApLedgerEntries
            .Where(x => x.CompanyId == companyId && x.SupplierId == supplierId)
            .OrderByDescending(x => x.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default) =>
        _context.ApLedgerEntries.CountAsync(x => x.CompanyId == companyId && x.SupplierId == supplierId, cancellationToken);
}
