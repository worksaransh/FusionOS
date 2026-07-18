using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Queries.SuggestMatchesForStatementLine;

public sealed class SuggestMatchesForStatementLineQueryHandler
    : IRequestHandler<SuggestMatchesForStatementLineQuery, IReadOnlyList<JournalEntryMatchCandidateDto>>
{
    private readonly IBankStatementLineRepository _statementLineRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;

    public SuggestMatchesForStatementLineQueryHandler(
        IBankStatementLineRepository statementLineRepository,
        IJournalEntryRepository journalEntryRepository)
    {
        _statementLineRepository = statementLineRepository;
        _journalEntryRepository = journalEntryRepository;
    }

    public async Task<IReadOnlyList<JournalEntryMatchCandidateDto>> Handle(SuggestMatchesForStatementLineQuery request, CancellationToken cancellationToken)
    {
        var line = await _statementLineRepository.GetByIdAsync(request.CompanyId, request.StatementLineId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bank statement line '{request.StatementLineId}' was not found.");

        var tolerance = Math.Abs(request.DateToleranceDays);
        var dateFrom = line.TransactionDate.AddDays(-tolerance);
        var dateTo = line.TransactionDate.AddDays(tolerance);

        // Match on the statement line's absolute amount: the line's sign is the bank's
        // (deposit +, withdrawal -), whereas a balanced JournalEntry's magnitude is
        // unsigned. Comparing magnitudes finds same-value candidates; the human confirms
        // direction/intent when they reconcile.
        var magnitude = Math.Abs(line.Amount);

        var candidates = await _journalEntryRepository.FindPostedByAmountWithinDateRangeAsync(
            request.CompanyId, magnitude, dateFrom, dateTo, cancellationToken);

        var alreadyMatched = await _statementLineRepository.GetMatchedJournalEntryIdsAsync(request.CompanyId, cancellationToken);
        var matchedSet = alreadyMatched.ToHashSet();

        return candidates
            .Where(e => !matchedSet.Contains(e.Id))
            .OrderBy(e => Math.Abs((e.EntryDate - line.TransactionDate).Ticks))
            .Select(e => new JournalEntryMatchCandidateDto(e.Id, e.EntryDate, e.Reference, e.TotalDebit))
            .ToList();
    }
}
