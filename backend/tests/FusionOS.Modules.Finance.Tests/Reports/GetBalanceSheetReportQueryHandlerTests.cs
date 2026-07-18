using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetBalanceSheetReport;
using FusionOS.Modules.Finance.Domain.Accounts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Reports;

/// <summary>Covers GetBalanceSheetReportQuery (Phase 2 closeout, 2026-07-18).</summary>
public class GetBalanceSheetReportQueryHandlerTests
{
    [Fact]
    public async Task Handle_SplitsAssetLiabilityEquityAndReportsBalanced()
    {
        var companyId = Guid.NewGuid();
        var cash = Account.Create(companyId, "1000", "Cash", AccountType.Asset, null);
        var payable = Account.Create(companyId, "2000", "Accounts Payable", AccountType.Liability, null);
        var equity = Account.Create(companyId, "3000", "Owner's Equity", AccountType.Equity, null);
        var revenue = Account.Create(companyId, "4000", "Revenue", AccountType.Revenue, null); // must be excluded from a Balance Sheet

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.GetPostedBalancesByAccountAsOfAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>
            {
                (cash.Id, 1000m, 0m),    // debit-normal: 1000 asset
                (payable.Id, 0m, 600m),  // credit-normal: 600 liability
                (equity.Id, 0m, 400m),   // credit-normal: 400 equity
                (revenue.Id, 0m, 300m),  // must not appear on the Balance Sheet
            });

        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account> { cash, payable, equity, revenue });

        var handler = new GetBalanceSheetReportQueryHandler(journalEntryRepository, accountRepository);

        var result = await handler.Handle(new GetBalanceSheetReportQuery(companyId, DateTimeOffset.UtcNow), CancellationToken.None);

        result.AssetLines.Should().ContainSingle(l => l.AccountId == cash.Id && l.Amount == 1000m);
        result.LiabilityLines.Should().ContainSingle(l => l.AccountId == payable.Id && l.Amount == 600m);
        result.EquityLines.Should().ContainSingle(l => l.AccountId == equity.Id && l.Amount == 400m);
        result.TotalAssets.Should().Be(1000m);
        result.TotalLiabilities.Should().Be(600m);
        result.TotalEquity.Should().Be(400m);
        result.IsBalanced.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAssetsDoNotEqualLiabilitiesPlusEquity_ReportsUnbalanced()
    {
        var companyId = Guid.NewGuid();
        var cash = Account.Create(companyId, "1000", "Cash", AccountType.Asset, null);
        var payable = Account.Create(companyId, "2000", "Accounts Payable", AccountType.Liability, null);

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.GetPostedBalancesByAccountAsOfAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>
            {
                (cash.Id, 1000m, 0m),
                (payable.Id, 0m, 100m),
            });
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account> { cash, payable });

        var handler = new GetBalanceSheetReportQueryHandler(journalEntryRepository, accountRepository);

        var result = await handler.Handle(new GetBalanceSheetReportQuery(companyId, DateTimeOffset.UtcNow), CancellationToken.None);

        result.IsBalanced.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNoPostedActivity_ReturnsEmptyBalancedReport()
    {
        var companyId = Guid.NewGuid();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.GetPostedBalancesByAccountAsOfAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>());
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account>());

        var handler = new GetBalanceSheetReportQueryHandler(journalEntryRepository, accountRepository);

        var result = await handler.Handle(new GetBalanceSheetReportQuery(companyId, DateTimeOffset.UtcNow), CancellationToken.None);

        result.AssetLines.Should().BeEmpty();
        result.IsBalanced.Should().BeTrue();
    }
}
