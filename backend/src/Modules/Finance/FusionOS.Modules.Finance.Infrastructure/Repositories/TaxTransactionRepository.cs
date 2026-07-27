using FusionOS.Modules.Finance.Application.TaxTransactions.Contracts;
using FusionOS.Modules.Finance.Domain.TaxTransactions;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class TaxTransactionRepository : ITaxTransactionRepository
{
    private readonly FinanceDbContext _context;

    public TaxTransactionRepository(FinanceDbContext context) => _context = context;

    public async Task AddAsync(TaxTransaction taxTransaction, CancellationToken cancellationToken = default) =>
        await _context.TaxTransactions.AddAsync(taxTransaction, cancellationToken);

    public Task<bool> ExistsForSourceAsync(Guid companyId, string sourceEntityType, Guid sourceEntityId, Guid taxRateId, CancellationToken cancellationToken = default) =>
        _context.TaxTransactions.AnyAsync(
            t => t.CompanyId == companyId
                && t.SourceEntityType == sourceEntityType
                && t.SourceEntityId == sourceEntityId
                && t.TaxRateId == taxRateId,
            cancellationToken);

    public async Task<IReadOnlyList<TaxTransaction>> ListInPeriodAsync(Guid companyId, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default) =>
        await _context.TaxTransactions
            .Where(t => t.CompanyId == companyId && t.TransactionDate >= periodStart && t.TransactionDate <= periodEnd)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
}
