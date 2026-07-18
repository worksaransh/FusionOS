using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class BankStatementLineRepository : IBankStatementLineRepository
{
    private readonly FinanceDbContext _context;

    public BankStatementLineRepository(FinanceDbContext context) => _context = context;

    public Task<BankStatementLine?> GetByIdAsync(Guid companyId, Guid statementLineId, CancellationToken cancellationToken = default) =>
        _context.BankStatementLines.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == statementLineId, cancellationToken);

    public async Task AddAsync(BankStatementLine line, CancellationToken cancellationToken = default) =>
        await _context.BankStatementLines.AddAsync(line, cancellationToken);

    public async Task<IReadOnlyList<BankStatementLine>> ListByBankAccountAsync(Guid companyId, Guid bankAccountId, bool? isReconciled, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, bankAccountId, isReconciled)
            .OrderByDescending(x => x.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountByBankAccountAsync(Guid companyId, Guid bankAccountId, bool? isReconciled, CancellationToken cancellationToken = default) =>
        Filtered(companyId, bankAccountId, isReconciled).CountAsync(cancellationToken);

    public async Task<(int TotalLines, int ReconciledCount, int UnreconciledCount, decimal UnreconciledTotalAmount)> GetReconciliationSummaryAsync(Guid companyId, Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        var lines = _context.BankStatementLines.Where(x => x.CompanyId == companyId && x.BankAccountId == bankAccountId);

        var totalLines = await lines.CountAsync(cancellationToken);
        var reconciledCount = await lines.CountAsync(x => x.IsReconciled, cancellationToken);
        var unreconciledTotalAmount = await lines.Where(x => !x.IsReconciled).SumAsync(x => x.Amount, cancellationToken);

        return (totalLines, reconciledCount, totalLines - reconciledCount, unreconciledTotalAmount);
    }

    public async Task<IReadOnlyList<Guid>> GetMatchedJournalEntryIdsAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        await _context.BankStatementLines
            .Where(x => x.CompanyId == companyId && x.MatchedJournalEntryId != null)
            .Select(x => x.MatchedJournalEntryId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

    private IQueryable<BankStatementLine> Filtered(Guid companyId, Guid bankAccountId, bool? isReconciled)
    {
        var query = _context.BankStatementLines.Where(x => x.CompanyId == companyId && x.BankAccountId == bankAccountId);
        if (isReconciled.HasValue)
            query = query.Where(x => x.IsReconciled == isReconciled.Value);
        return query;
    }
}
