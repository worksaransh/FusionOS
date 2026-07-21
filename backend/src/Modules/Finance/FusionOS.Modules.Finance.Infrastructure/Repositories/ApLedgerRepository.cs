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

    public Task<decimal> SumAmountByPurchaseOrderAsync(Guid companyId, Guid purchaseOrderId, CancellationToken cancellationToken = default) =>
        _context.ApLedgerEntries
            .Where(x => x.CompanyId == companyId && x.PurchaseOrderId == purchaseOrderId)
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

    public async Task<IReadOnlyList<(Guid SupplierId, decimal Balance, DateTimeOffset OldestChargeDate)>> GetOutstandingSupplierBalancesAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var grouped = await _context.ApLedgerEntries
            .Where(x => x.CompanyId == companyId)
            .GroupBy(x => x.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                Balance = g.Sum(x => x.Amount),
                OldestChargeDate = g.Min(x => x.TransactionDate),
            })
            .Where(g => g.Balance != 0)
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.SupplierId, g.Balance, g.OldestChargeDate)).ToList();
    }
}
