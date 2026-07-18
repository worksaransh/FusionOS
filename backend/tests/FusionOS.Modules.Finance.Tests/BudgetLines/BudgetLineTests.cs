using FluentAssertions;
using FusionOS.Modules.Finance.Domain.BudgetLines;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BudgetLines;

public class BudgetLineTests
{
    [Fact]
    public void Create_WithValidData_CreatesBudgetLine()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var costCenterId = Guid.NewGuid();

        var budgetLine = BudgetLine.Create(companyId, budgetId, accountId, costCenterId, 1000m, "  Marketing spend  ");

        budgetLine.BudgetId.Should().Be(budgetId);
        budgetLine.AccountId.Should().Be(accountId);
        budgetLine.CostCenterId.Should().Be(costCenterId);
        budgetLine.BudgetedAmount.Should().Be(1000m);
        budgetLine.Notes.Should().Be("Marketing spend");
    }

    [Fact]
    public void Create_WithoutCostCenterOrNotes_AllowsNulls()
    {
        var budgetLine = BudgetLine.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, 0m, null);

        budgetLine.CostCenterId.Should().BeNull();
        budgetLine.Notes.Should().BeNull();
        budgetLine.BudgetedAmount.Should().Be(0m);
    }

    [Fact]
    public void Create_WithEmptyBudgetId_Throws()
    {
        var act = () => BudgetLine.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), null, 100m, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyAccountId_Throws()
    {
        var act = () => BudgetLine.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, null, 100m, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeBudgetedAmount_Throws()
    {
        var act = () => BudgetLine.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, -1m, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateAmount_WithValidData_UpdatesAmountAndNotes()
    {
        var budgetLine = BudgetLine.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, 100m, "Original");

        budgetLine.UpdateAmount(250m, "Corrected");

        budgetLine.BudgetedAmount.Should().Be(250m);
        budgetLine.Notes.Should().Be("Corrected");
    }

    [Fact]
    public void UpdateAmount_WithNegativeAmount_Throws()
    {
        var budgetLine = BudgetLine.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, 100m, null);

        var act = () => budgetLine.UpdateAmount(-5m, null);

        act.Should().Throw<ArgumentException>();
    }
}
