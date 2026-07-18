using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetTrialBalance;

public sealed class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, TrialBalanceReportDto>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;

    public GetTrialBalanceQueryHandler(IJournalEntryRepository journalEntryRepository, IAccountRepository accountRepository)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<TrialBalanceReportDto> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var balances = await _journalEntryRepository.GetPostedBalancesByAccountAsOfAsync(request.CompanyId, request.AsOfDate, cancellationToken);

        var lines = new List<TrialBalanceLineDto>();
        foreach (var b in balances)
        {
            var account = await _accountRepository.GetByIdAsync(request.CompanyId, b.AccountId, cancellationToken)
                ?? throw new KeyNotFoundException($"Account '{b.AccountId}' referenced by a posted journal entry was not found.");

            lines.Add(new TrialBalanceLineDto(
                b.AccountId,
                account.Code,
                account.Name,
                b.TotalDebit,
                b.TotalCredit,
                b.TotalDebit - b.TotalCredit));
        }

        var ordered = lines.OrderBy(l => l.AccountCode, StringComparer.Ordinal).ToList();
        var totalDebit = ordered.Sum(l => l.TotalDebit);
        var totalCredit = ordered.Sum(l => l.TotalCredit);

        return new TrialBalanceReportDto(request.AsOfDate, ordered, totalDebit, totalCredit, totalDebit == totalCredit);
    }
}
