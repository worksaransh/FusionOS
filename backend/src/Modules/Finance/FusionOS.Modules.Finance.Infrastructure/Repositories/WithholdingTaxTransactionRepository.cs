using FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Contracts;
using FusionOS.Modules.Finance.Domain.WithholdingTaxTransactions;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class WithholdingTaxTransactionRepository : IWithholdingTaxTransactionRepository
{
    private readonly FinanceDbContext _context;

    public WithholdingTaxTransactionRepository(FinanceDbContext context) => _context = context;

    public async Task AddAsync(WithholdingTaxTransaction transaction, CancellationToken cancellationToken = default) =>
        await _context.WithholdingTaxTransactions.AddAsync(transaction, cancellationToken);

    public async Task<IReadOnlyList<WithholdingTaxTransaction>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.WithholdingTaxTransactions
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.WithholdingTaxTransactions.CountAsync(t => t.CompanyId == companyId, cancellationToken);
}
