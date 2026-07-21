using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetTrialBalance;
using FusionOS.Modules.Finance.Domain.Accounts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Reports;

public class GetTrialBalanceQueryHandlerTests
{
    [Fact]
    public async Task Handle_BuildsBalancedTrialBalanceOrderedByAccountCode()
    {
        var companyId = Guid.NewGuid();
        var cash = Account.Create(companyId, "1000", "Cash", AccountType.Asset, null);
        var revenue = Account.Create(companyId, "4000", "Revenue", AccountType.Revenue, null);

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.GetPostedBalancesByAccountAsOfAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>
            {
                (revenue.Id, 0m, 500m),
                (cash.Id, 500m, 0m),
            });

        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account> { cash, revenue });

        var handler = new GetTrialBalanceQueryHandler(journalEntryRepository, accountRepository);

        var result = await handler.Handle(new GetTrialBalanceQuery(companyId, DateTimeOffset.UtcNow), CancellationToken.None);

        result.Lines.Should().HaveCount(2);
        result.Lines[0].AccountCode.Should().Be("1000"); // ordered by code
        result.Lines[0].NetBalance.Should().Be(500m);
        result.Lines[1].NetBalance.Should().Be(-500m);
        result.TotalDebit.Should().Be(500m);
        result.TotalCredit.Should().Be(500m);
        result.IsBalanced.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoPostedActivity_ReturnsEmptyBalancedReport()
    {
        var companyId = Guid.NewGuid();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.GetPostedBalancesByAccountAsOfAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, decimal)>());
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account>());

        var handler = new GetTrialBalanceQueryHandler(journalEntryRepository, accountRepository);

        var result = await handler.Handle(new GetTrialBalanceQuery(companyId, DateTimeOffset.UtcNow), CancellationToken.None);

        result.Lines.Should().BeEmpty();
        result.TotalDebit.Should().Be(0m);
        result.TotalCredit.Should().Be(0m);
        result.IsBalanced.Should().BeTrue();
    }
}
