using FluentAssertions;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.RecurringJournalEntries;

public class RecurringJournalEntryTemplateTests
{
    private static RecurringJournalEntryTemplateLineInput[] BalancedLines(decimal amount = 100m) => new[]
    {
        new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), amount, 0m),
        new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 0m, amount),
    };

    [Fact]
    public void Create_WithBalancedLines_IsActiveWithLines()
    {
        var template = RecurringJournalEntryTemplate.Create(Guid.NewGuid(), "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, BalancedLines());

        template.IsActive.Should().BeTrue();
        template.Lines.Should().HaveCount(2);
        template.TotalDebit.Should().Be(template.TotalCredit);
    }

    [Fact]
    public void Create_WithUnbalancedLines_Throws()
    {
        var lines = new[]
        {
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 100m, 0m),
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 0m, 50m),
        };

        var act = () => RecurringJournalEntryTemplate.Create(Guid.NewGuid(), "Unbalanced", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, lines);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithFewerThanTwoLines_Throws()
    {
        var lines = new[] { new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 100m, 0m) };

        var act = () => RecurringJournalEntryTemplate.Create(Guid.NewGuid(), "Too few lines", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, lines);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyReference_Throws()
    {
        var act = () => RecurringJournalEntryTemplate.Create(Guid.NewGuid(), " ", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, BalancedLines());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithBalancedLines_ReplacesLines()
    {
        var template = RecurringJournalEntryTemplate.Create(Guid.NewGuid(), "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, BalancedLines(100m));

        template.UpdateDetails("Monthly rent (revised)", RecurringJournalEntryFrequency.Quarterly, DateTimeOffset.UtcNow.AddDays(10), BalancedLines(200m));

        template.Reference.Should().Be("Monthly rent (revised)");
        template.Frequency.Should().Be(RecurringJournalEntryFrequency.Quarterly);
        template.TotalDebit.Should().Be(200m);
    }

    [Fact]
    public void UpdateDetails_WithUnbalancedLines_Throws()
    {
        var template = RecurringJournalEntryTemplate.Create(Guid.NewGuid(), "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, BalancedLines());
        var unbalanced = new[]
        {
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 100m, 0m),
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 0m, 50m),
        };

        var act = () => template.UpdateDetails("Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, unbalanced);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateDetails_WhenDeactivated_Throws()
    {
        var template = RecurringJournalEntryTemplate.Create(Guid.NewGuid(), "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, BalancedLines());
        template.Deactivate();

        var act = () => template.UpdateDetails("Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, BalancedLines());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Throws()
    {
        var template = RecurringJournalEntryTemplate.Create(Guid.NewGuid(), "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, BalancedLines());
        template.Deactivate();

        var act = () => template.Deactivate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(RecurringJournalEntryFrequency.Monthly, 1, 0)]
    [InlineData(RecurringJournalEntryFrequency.Quarterly, 3, 0)]
    [InlineData(RecurringJournalEntryFrequency.Annual, 0, 1)]
    public void AdvanceNextDueDate_AdvancesByFrequency(RecurringJournalEntryFrequency frequency, int expectedMonths, int expectedYears)
    {
        var start = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var template = RecurringJournalEntryTemplate.Create(Guid.NewGuid(), "Recurring", frequency, start, BalancedLines());

        template.AdvanceNextDueDate();

        var expected = start.AddMonths(expectedMonths).AddYears(expectedYears);
        template.NextDueDate.Should().Be(expected);
    }
}
