using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetBalanceSheetReport;

public sealed class GetBalanceSheetReportQueryHandler : IRequestHandler<GetBalanceSheetReportQuery, BalanceSheetReportDto>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;

    public GetBalanceSheetReportQueryHandler(IJournalEntryRepository journalEntryRepository, IAccountRepository accountRepository)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<BalanceSheetReportDto> Handle(GetBalanceSheetReportQuery request, CancellationToken cancellationToken)
    {
        var balances = await _journalEntryRepository.GetPostedBalancesByAccountAsOfAsync(request.CompanyId, request.AsOfDate, cancellationToken);
        var accounts = (await _accountRepository.ListAllAsync(request.CompanyId, cancellationToken)).ToDictionary(a => a.Id);

        var assetLines = new List<BalanceSheetLineDto>();
        var liabilityLines = new List<BalanceSheetLineDto>();
        var equityLines = new List<BalanceSheetLineDto>();

        foreach (var b in balances)
        {
            if (!accounts.TryGetValue(b.AccountId, out var account))
                continue; // an account referenced only by a posted line that predates a since-corrected chart is not this report's concern to repair

            switch (account.AccountType)
            {
                case AccountType.Asset:
                    assetLines.Add(new BalanceSheetLineDto(account.Id, account.Code, account.Name, b.TotalDebit - b.TotalCredit));
                    break;
                case AccountType.Liability:
                    liabilityLines.Add(new BalanceSheetLineDto(account.Id, account.Code, account.Name, b.TotalCredit - b.TotalDebit));
                    break;
                case AccountType.Equity:
                    equityLines.Add(new BalanceSheetLineDto(account.Id, account.Code, account.Name, b.TotalCredit - b.TotalDebit));
                    break;
            }
        }

        assetLines = assetLines.OrderBy(l => l.AccountCode, StringComparer.Ordinal).ToList();
        liabilityLines = liabilityLines.OrderBy(l => l.AccountCode, StringComparer.Ordinal).ToList();
        equityLines = equityLines.OrderBy(l => l.AccountCode, StringComparer.Ordinal).ToList();

        var totalAssets = assetLines.Sum(l => l.Amount);
        var totalLiabilities = liabilityLines.Sum(l => l.Amount);
        var totalEquity = equityLines.Sum(l => l.Amount);

        return new BalanceSheetReportDto(
            request.AsOfDate, assetLines, liabilityLines, equityLines,
            totalAssets, totalLiabilities, totalEquity,
            totalAssets == totalLiabilities + totalEquity);
    }
}
