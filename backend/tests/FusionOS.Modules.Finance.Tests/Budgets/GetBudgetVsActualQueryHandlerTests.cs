using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Queries.GetBudgetVsActual;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using FusionOS.Modules.Finance.Domain.Budgets;
using FusionOS.Modules.Finance.Domain.BudgetLines;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Budgets;

public class GetBudgetVsActualQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenBudgetExists_ReturnsBudgetedActualAndVarianceForEachLine()
    {
        var companyId = Guid.NewGuid();
        var periodStart = DateTimeOffset.UtcNow;
        var periodEnd = periodStart.AddDays(90);
        var budget = Budget.Create(companyId, "Q Budget", periodStart, periodEnd);
        var account = Account.Create(companyId, "6000", "Office Supplies", AccountType.Expense, null);
        var budgetLine = BudgetLine.Create(companyId, budget.Id, account.Id, null, 1000m, null);

        var budgetRepository = Substitute.For<IBudgetRepository>();
        budgetRepository.GetByIdAsync(companyId, budget.Id, Arg.Any<CancellationToken>()).Returns(budget);

        var budgetLineRepository = Substitute.For<IBudgetLineRepository>();
        budgetLineRepository.ListAllByBudgetAsync(companyId, budget.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { budgetLine });

        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account> { account });

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.SumPostedAmountByAccountAsync(companyId, account.Id, periodStart, periodEnd, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(750m);

        var handler = new GetBudgetVsActualQueryHandler(budgetRepository, budgetLineRepository, accountRepository, journalEntryRepository);

        var result = await handler.Handle(new GetBudgetVsActualQuery(companyId, budget.Id), CancellationToken.None);

        result.Should().ContainSingle();
        var row = result[0];
        row.AccountId.Should().Be(account.Id);
        row.AccountCode.Should().Be("6000");
        row.AccountName.Should().Be("Office Supplies");
        row.BudgetedAmount.Should().Be(1000m);
        row.ActualAmount.Should().Be(750m);
        row.VarianceAmount.Should().Be(-250m);
    }

    [Fact]
    public async Task Handle_WhenBudgetLineHasCostCenter_RestrictsActualToThatCostCenter()
    {
        var companyId = Guid.NewGuid();
        var periodStart = DateTimeOffset.UtcNow;
        var periodEnd = periodStart.AddDays(90);
        var costCenterId = Guid.NewGuid();
        var budget = Budget.Create(companyId, "Q Budget", periodStart, periodEnd);
        var account = Account.Create(companyId, "6000", "Office Supplies", AccountType.Expense, null);
        var budgetLine = BudgetLine.Create(companyId, budget.Id, account.Id, costCenterId, 1000m, null);

        var budgetRepository = Substitute.For<IBudgetRepository>();
        budgetRepository.GetByIdAsync(companyId, budget.Id, Arg.Any<CancellationToken>()).Returns(budget);

        var budgetLineRepository = Substitute.For<IBudgetLineRepository>();
        budgetLineRepository.ListAllByBudgetAsync(companyId, budget.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { budgetLine });

        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<Account> { account });

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.SumPostedAmountByAccountAsync(companyId, account.Id, periodStart, periodEnd, costCenterId, Arg.Any<CancellationToken>())
            .Returns(500m);

        var handler = new GetBudgetVsActualQueryHandler(budgetRepository, budgetLineRepository, accountRepository, journalEntryRepository);

        var result = await handler.Handle(new GetBudgetVsActualQuery(companyId, budget.Id), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].CostCenterId.Should().Be(costCenterId);
        result[0].ActualAmount.Should().Be(500m);
        await journalEntryRepository.Received(1)
            .SumPostedAmountByAccountAsync(companyId, account.Id, periodStart, periodEnd, costCenterId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var budgetRepository = Substitute.For<IBudgetRepository>();
        budgetRepository.GetByIdAsync(companyId, budgetId, Arg.Any<CancellationToken>()).Returns((Budget?)null);

        var handler = new GetBudgetVsActualQueryHandler(
            budgetRepository,
            Substitute.For<IBudgetLineRepository>(),
            Substitute.For<IAccountRepository>(),
            Substitute.For<IJournalEntryRepository>());

        var act = () => handler.Handle(new GetBudgetVsActualQuery(companyId, budgetId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
