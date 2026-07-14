using FluentAssertions;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.JournalEntries;

public class JournalEntryTests
{
    private static JournalEntryLineInput DebitLine(decimal amount) => new(Guid.NewGuid(), amount, 0m, "debit");
    private static JournalEntryLineInput CreditLine(decimal amount) => new(Guid.NewGuid(), 0m, amount, "credit");

    [Fact]
    public void Create_WhenBalanced_Succeeds()
    {
        var entry = JournalEntry.Create(Guid.NewGuid(), "REF-1", new[] { DebitLine(100m), CreditLine(100m) });

        entry.Status.Should().Be(JournalEntryStatus.Draft);
        entry.TotalDebit.Should().Be(100m);
        entry.TotalCredit.Should().Be(100m);
        entry.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void Create_WhenUnbalanced_Throws()
    {
        var act = () => JournalEntry.Create(Guid.NewGuid(), "REF-2", new[] { DebitLine(100m), CreditLine(50m) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithFewerThanTwoLines_Throws()
    {
        var act = () => JournalEntry.Create(Guid.NewGuid(), "REF-3", new[] { DebitLine(100m) });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Post_WhenDraft_TransitionsToPosted()
    {
        var entry = JournalEntry.Create(Guid.NewGuid(), null, new[] { DebitLine(50m), CreditLine(50m) });

        entry.Post();

        entry.Status.Should().Be(JournalEntryStatus.Posted);
    }

    [Fact]
    public void Post_WhenAlreadyPosted_Throws()
    {
        var entry = JournalEntry.Create(Guid.NewGuid(), null, new[] { DebitLine(50m), CreditLine(50m) });
        entry.Post();

        var act = () => entry.Post();

        act.Should().Throw<InvalidOperationException>();
    }
}
