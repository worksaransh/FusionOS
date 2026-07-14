using FluentAssertions;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.JournalEntries;

public class JournalEntryLineTests
{
    [Fact]
    public void Create_WithBothDebitAndCredit_Throws()
    {
        var act = () => JournalEntry.Create(Guid.NewGuid(), null, new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 10m, 10m, null),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 20m, null),
        });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNeitherDebitNorCredit_Throws()
    {
        var act = () => JournalEntry.Create(Guid.NewGuid(), null, new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 0m, null),
            new JournalEntryLineInput(Guid.NewGuid(), 10m, 0m, null),
        });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeAmount_Throws()
    {
        var act = () => JournalEntry.Create(Guid.NewGuid(), null, new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), -5m, 0m, null),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 5m, null),
        });

        act.Should().Throw<ArgumentException>();
    }
}
