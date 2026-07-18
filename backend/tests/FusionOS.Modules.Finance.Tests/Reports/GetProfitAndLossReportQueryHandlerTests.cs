using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetProfitAndLossReport;
using FusionOS.Modules.Finance.Domain.Accounts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Reports;

/// <summary>Covers GetProfitAndLossReportQuery (Phase 2 closeout, 2026-07-18).</summary>
public class GetProfitAndLossReportQueryHandlerTests
{
    [Fact]
    public async Task Handle_SplitsRevenueAndExpenseAndComputesNetIncome()
    {
        var companyId = Guid.NewGuid();
        var revenue = Account.Create(companyId, "4000", "Sales Revenue", AccountType.Revenue, null);
        var expense = Account.Create(companyId, "5000", "COGS", AccountType.Expense, null);
        var cash = Account.Create(companyId, "1000", "Cash", AccountType.Asset, null); // must be excluded from a P&L

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.GetPostedBalancesByAccountInRangeAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>
            {
                (revenue.Id, 0m, 1000m),  // credit-normal: 1000 revenue
                (expense.Id, 400m, 0m),   // debit-normal: 400 expense
                (cash.Id, 600m, 0m),      // Asset activity — must not appear on the P&L
            });

        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account> { revenue, expense, cash });

        var handler = new GetProfitAndLossReportQueryHandler(journalEntryRepository, accountRepository);
        var periodStart = DateTimeOffset.UtcNow.AddMonths(-1);
        var periodEnd = DateTimeOffset.UtcNow;

        var result = await handler.Handle(new GetProfitAndLossReportQuery(companyId, periodStart, periodEnd), CancellationToken.None);

        result.RevenueLines.Should().ContainSingle(l => l.AccountId == revenue.Id && l.Amount == 1000m);
        result.ExpenseLines.Should().ContainSingle(l => l.AccountId == expense.Id && l.Amount == 400m);
        result.TotalRevenue.Should().Be(1000m);
        result.TotalExpenses.Should().Be(400m);
        result.NetIncome.Should().Be(600m);
    }

    [Fact]
    public async Task Handle_WithNoPostedActivity_ReturnsEmptyZeroReport()
    {
        var companyId = Guid.NewGuid();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.GetPostedBalancesByAccountInRangeAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>());
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account>());

        var handler = new GetProfitAndLossReportQueryHandler(journalEntryRepository, accountRepository);

        var result = await handler.Handle(new GetProfitAndLossReportQuery(companyId, DateTimeOffset.UtcNow.AddMonths(-1), DateTimeOffset.UtcNow), CancellationToken.None);

        result.RevenueLines.Should().BeEmpty();
        result.ExpenseLines.Should().BeEmpty();
        result.NetIncome.Should().Be(0m);
    }
}
