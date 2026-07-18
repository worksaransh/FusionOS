using FluentAssertions;
using FusionOS.Modules.Finance.Domain.Budgets;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Budgets;

public class BudgetTests
{
    [Fact]
    public void Create_WithValidData_CreatesActiveBudget()
    {
        var periodStart = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

        var budget = Budget.Create(Guid.NewGuid(), "  FY2026 Operating Budget  ", periodStart, periodEnd);

        budget.Name.Should().Be("FY2026 Operating Budget");
        budget.PeriodStart.Should().Be(periodStart);
        budget.PeriodEnd.Should().Be(periodEnd);
        budget.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_Throws(string invalidName)
    {
        var act = () => Budget.Create(Guid.NewGuid(), invalidName, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithPeriodEndNotAfterPeriodStart_Throws()
    {
        var start = DateTimeOffset.UtcNow;

        var act = () => Budget.Create(Guid.NewGuid(), "Budget", start, start);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithPeriodEndBeforePeriodStart_Throws()
    {
        var start = DateTimeOffset.UtcNow;

        var act = () => Budget.Create(Guid.NewGuid(), "Budget", start, start.AddDays(-1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var budget = Budget.Create(Guid.NewGuid(), "Old Name", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        var newStart = DateTimeOffset.UtcNow.AddDays(1);
        var newEnd = newStart.AddDays(60);

        budget.UpdateDetails("New Name", newStart, newEnd);

        budget.Name.Should().Be("New Name");
        budget.PeriodStart.Should().Be(newStart);
        budget.PeriodEnd.Should().Be(newEnd);
    }

    [Fact]
    public void UpdateDetails_WithPeriodEndNotAfterPeriodStart_Throws()
    {
        var budget = Budget.Create(Guid.NewGuid(), "Budget", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        var start = DateTimeOffset.UtcNow;

        var act = () => budget.UpdateDetails("Budget", start, start);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var budget = Budget.Create(Guid.NewGuid(), "Budget", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));

        budget.Deactivate();

        budget.IsActive.Should().BeFalse();
    }
}
