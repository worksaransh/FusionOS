using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetCashFlowStatementReport;
using FusionOS.Modules.Finance.Domain.Accounts;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Reports;

/// <summary>Covers GetCashFlowStatementReportQuery (indirect method, 2026-07-22 — see CashFlowStatementReportDto's doc comment for the documented scope/limitations this test scenario is deliberately built to stay within).</summary>
public class GetCashFlowStatementReportQueryHandlerTests
{
    private static (IJournalEntryRepository JournalEntryRepository, IAccountRepository AccountRepository, IArLedgerRepository ArLedgerRepository, IApLedgerRepository ApLedgerRepository, IFixedAssetRepository FixedAssetRepository) Mocks() =>
        (Substitute.For<IJournalEntryRepository>(), Substitute.For<IAccountRepository>(), Substitute.For<IArLedgerRepository>(), Substitute.For<IApLedgerRepository>(), Substitute.For<IFixedAssetRepository>());

    [Fact]
    public async Task Handle_ComputesThreeSectionsThatSumToNetChangeInCash()
    {
        var companyId = Guid.NewGuid();
        var periodStart = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero);

        var revenue = Account.Create(companyId, "4000", "Sales Revenue", AccountType.Revenue, null);
        var expense = Account.Create(companyId, "5000", "COGS", AccountType.Expense, null);
        var equity = Account.Create(companyId, "3000", "Owner's Equity", AccountType.Equity, null);
        var assetAccount = Account.Create(companyId, "1500", "Vehicles", AccountType.Asset, null);

        var (journalEntryRepository, accountRepository, arLedgerRepository, apLedgerRepository, fixedAssetRepository) = Mocks();

        // Net income = 1000 revenue - 400 expense = 600. Equity grew by 200 (a capital contribution booked directly to Equity, no closing entry involved per this codebase's confirmed absence of one).
        journalEntryRepository.GetPostedBalancesByAccountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>
            {
                (revenue.Id, 0m, 1000m),
                (expense.Id, 400m, 0m),
                (equity.Id, 0m, 200m),
            });
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account> { revenue, expense, equity, assetAccount });

        // AR grew by 300 during the period (revenue recognized, not yet collected) -> operating adjustment -300.
        arLedgerRepository.SumAmountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>()).Returns(300m);
        // AP grew by 100 during the period (expense recognized, not yet paid) -> operating adjustment +100.
        apLedgerRepository.SumAmountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>()).Returns(100m);

        // One fixed asset acquired mid-period for 500 cash -> investing outflow -500.
        var acquiredAsset = FixedAsset.Create(companyId, "FA-1", "Delivery Van", assetAccount.Id, null, null, new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero), 500m, 50m, 36);
        fixedAssetRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<FixedAsset> { acquiredAsset });

        var handler = new GetCashFlowStatementReportQueryHandler(journalEntryRepository, accountRepository, arLedgerRepository, apLedgerRepository, fixedAssetRepository);

        var result = await handler.Handle(new GetCashFlowStatementReportQuery(companyId, periodStart, periodEnd), CancellationToken.None);

        result.NetIncome.Should().Be(600m);
        result.NetCashFromOperatingActivities.Should().Be(600m - 300m + 100m); // 400
        result.NetCashFromInvestingActivities.Should().Be(-500m);
        result.NetCashFromFinancingActivities.Should().Be(200m);

        var expectedNetChange = result.NetCashFromOperatingActivities + result.NetCashFromInvestingActivities + result.NetCashFromFinancingActivities;
        result.NetChangeInCash.Should().Be(expectedNetChange);
        result.NetChangeInCash.Should().Be(100m); // 400 - 500 + 200

        // In this deliberately well-behaved scenario (every Balance-Sheet account that moved is Cash-implicit, AR, AP, a FixedAsset acquisition, or Equity — see DTO doc comment), the indirect-method total also ties to the actual net cash impact of the period's activity:
        // cash paid out for the asset (-500) + cash collected from operations after AR/AP timing (net income 600, less 300 stuck in AR, plus 100 not yet paid out via AP = 400) + cash contributed to equity (+200) = 100.
    }

    [Fact]
    public async Task Handle_SurfacesDisposalsInPeriodAsAnUnvaluedInformationalLine()
    {
        var companyId = Guid.NewGuid();
        var periodStart = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero);

        var assetAccount = Account.Create(companyId, "1500", "Vehicles", AccountType.Asset, null);
        var (journalEntryRepository, accountRepository, arLedgerRepository, apLedgerRepository, fixedAssetRepository) = Mocks();

        journalEntryRepository.GetPostedBalancesByAccountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>());
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account> { assetAccount });
        arLedgerRepository.SumAmountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>()).Returns(0m);
        apLedgerRepository.SumAmountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>()).Returns(0m);

        var oldAsset = FixedAsset.Create(companyId, "FA-2", "Old Forklift", assetAccount.Id, null, null, new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), 1000m, 100m, 60);
        oldAsset.Dispose(new DateTimeOffset(2026, 6, 20, 0, 0, 0, TimeSpan.Zero));
        fixedAssetRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<FixedAsset> { oldAsset });

        var handler = new GetCashFlowStatementReportQueryHandler(journalEntryRepository, accountRepository, arLedgerRepository, apLedgerRepository, fixedAssetRepository);

        var result = await handler.Handle(new GetCashFlowStatementReportQuery(companyId, periodStart, periodEnd), CancellationToken.None);

        result.InvestingLines.Should().Contain(l => l.Label.Contains("Disposal") && l.Amount == 0m);
        result.NetCashFromInvestingActivities.Should().Be(0m); // acquisition line (0, nothing acquired) + disposal line (0, proceeds not tracked)
    }

    [Fact]
    public async Task Handle_WithNoActivity_ReturnsAllZeroReport()
    {
        var companyId = Guid.NewGuid();
        var periodStart = DateTimeOffset.UtcNow.AddMonths(-1);
        var periodEnd = DateTimeOffset.UtcNow;

        var (journalEntryRepository, accountRepository, arLedgerRepository, apLedgerRepository, fixedAssetRepository) = Mocks();
        journalEntryRepository.GetPostedBalancesByAccountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>()).Returns(new List<(Guid, decimal, decimal)>());
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account>());
        arLedgerRepository.SumAmountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>()).Returns(0m);
        apLedgerRepository.SumAmountInRangeAsync(companyId, periodStart, periodEnd, Arg.Any<CancellationToken>()).Returns(0m);
        fixedAssetRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<FixedAsset>());

        var handler = new GetCashFlowStatementReportQueryHandler(journalEntryRepository, accountRepository, arLedgerRepository, apLedgerRepository, fixedAssetRepository);

        var result = await handler.Handle(new GetCashFlowStatementReportQuery(companyId, periodStart, periodEnd), CancellationToken.None);

        result.NetIncome.Should().Be(0m);
        result.NetCashFromOperatingActivities.Should().Be(0m);
        result.NetCashFromInvestingActivities.Should().Be(0m);
        result.NetCashFromFinancingActivities.Should().Be(0m);
        result.NetChangeInCash.Should().Be(0m);
    }
}
