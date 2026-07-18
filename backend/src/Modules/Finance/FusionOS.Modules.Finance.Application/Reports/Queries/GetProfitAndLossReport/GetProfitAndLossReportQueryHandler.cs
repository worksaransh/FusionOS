using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetProfitAndLossReport;

public sealed class GetProfitAndLossReportQueryHandler : IRequestHandler<GetProfitAndLossReportQuery, ProfitAndLossReportDto>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;

    public GetProfitAndLossReportQueryHandler(IJournalEntryRepository journalEntryRepository, IAccountRepository accountRepository)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<ProfitAndLossReportDto> Handle(GetProfitAndLossReportQuery request, CancellationToken cancellationToken)
    {
        var balances = await _journalEntryRepository.GetPostedBalancesByAccountInRangeAsync(request.CompanyId, request.PeriodStart, request.PeriodEnd, cancellationToken);
        var accounts = (await _accountRepository.ListAllAsync(request.CompanyId, cancellationToken)).ToDictionary(a => a.Id);

        var revenueLines = new List<ProfitAndLossLineDto>();
        var expenseLines = new List<ProfitAndLossLineDto>();

        foreach (var b in balances)
        {
            if (!accounts.TryGetValue(b.AccountId, out var account))
                continue; // an account referenced only by a posted line that predates a since-corrected chart is not this report's concern to repair

            switch (account.AccountType)
            {
                case AccountType.Revenue:
                    revenueLines.Add(new ProfitAndLossLineDto(account.Id, account.Code, account.Name, b.TotalCredit - b.TotalDebit));
                    break;
                case AccountType.Expense:
                    expenseLines.Add(new ProfitAndLossLineDto(account.Id, account.Code, account.Name, b.TotalDebit - b.TotalCredit));
                    break;
            }
        }

        revenueLines = revenueLines.OrderBy(l => l.AccountCode, StringComparer.Ordinal).ToList();
        expenseLines = expenseLines.OrderBy(l => l.AccountCode, StringComparer.Ordinal).ToList();

        var totalRevenue = revenueLines.Sum(l => l.Amount);
        var totalExpenses = expenseLines.Sum(l => l.Amount);

        return new ProfitAndLossReportDto(request.PeriodStart, request.PeriodEnd, revenueLines, expenseLines, totalRevenue, totalExpenses, totalRevenue - totalExpenses);
    }
}
