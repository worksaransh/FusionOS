using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Domain.Receivables;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class ArLedgerRepository : IArLedgerRepository
{
    private readonly FinanceDbContext _context;

    public ArLedgerRepository(FinanceDbContext context) => _context = context;

    public async Task AddAsync(ArLedgerEntry entry, CancellationToken cancellationToken = default) =>
        await _context.ArLedgerEntries.AddAsync(entry, cancellationToken);

    public Task<decimal> SumAmountAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default) =>
        _context.ArLedgerEntries
            .Where(x => x.CompanyId == companyId && x.CustomerId == customerId)
            .SumAsync(x => x.Amount, cancellationToken);

    public Task<decimal> SumAmountByInvoiceAsync(Guid companyId, Guid invoiceId, CancellationToken cancellationToken = default) =>
        _context.ArLedgerEntries
            .Where(x => x.CompanyId == companyId && x.InvoiceId == invoiceId)
            .SumAsync(x => x.Amount, cancellationToken);

    public async Task<IReadOnlyList<ArLedgerEntry>> ListAsync(Guid companyId, Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.ArLedgerEntries
            .Where(x => x.CompanyId == companyId && x.CustomerId == customerId)
            .OrderByDescending(x => x.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default) =>
        _context.ArLedgerEntries.CountAsync(x => x.CompanyId == companyId && x.CustomerId == customerId, cancellationToken);

    public async Task<IReadOnlyList<(Guid CustomerId, Guid InvoiceId, decimal Balance, DateTimeOffset ChargeDate)>> GetOutstandingInvoiceBalancesAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var grouped = await _context.ArLedgerEntries
            .Where(x => x.CompanyId == companyId)
            .GroupBy(x => new { x.CustomerId, x.InvoiceId })
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.InvoiceId,
                Balance = g.Sum(x => x.Amount),
                ChargeDate = g.Min(x => x.TransactionDate),
            })
            .Where(g => g.Balance != 0)
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.CustomerId, g.InvoiceId, g.Balance, g.ChargeDate)).ToList();
    }
}
