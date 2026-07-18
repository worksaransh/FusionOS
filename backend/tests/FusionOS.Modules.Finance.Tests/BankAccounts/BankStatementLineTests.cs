using FluentAssertions;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class BankStatementLineTests
{
    [Fact]
    public void Create_WithPositiveAmount_RepresentsADeposit()
    {
        var line = BankStatementLine.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 500m, "Customer wire");

        line.Amount.Should().Be(500m);
        line.IsReconciled.Should().BeFalse();
        line.ReconciledAt.Should().BeNull();
        line.MatchedJournalEntryId.Should().BeNull();
    }

    [Fact]
    public void Create_WithNegativeAmount_RepresentsAWithdrawal()
    {
        var line = BankStatementLine.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, -125.50m, "Bank fee");

        line.Amount.Should().Be(-125.50m);
    }

    [Fact]
    public void Create_WithZeroAmount_Throws()
    {
        var act = () => BankStatementLine.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 0m, "Zero-amount line");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyBankAccountId_Throws()
    {
        var act = () => BankStatementLine.Create(Guid.NewGuid(), Guid.Empty, DateTimeOffset.UtcNow, 100m, "Deposit");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyDescription_Throws(string invalidDescription)
    {
        var act = () => BankStatementLine.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 100m, invalidDescription);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Reconcile_WithMatchedJournalEntry_SetsReconciledState()
    {
        var line = BankStatementLine.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 100m, "Deposit");
        var journalEntryId = Guid.NewGuid();

        line.Reconcile(journalEntryId);

        line.IsReconciled.Should().BeTrue();
        line.ReconciledAt.Should().NotBeNull();
        line.MatchedJournalEntryId.Should().Be(journalEntryId);
    }

    [Fact]
    public void Reconcile_WithoutMatchedJournalEntry_StillReconciles()
    {
        var line = BankStatementLine.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 100m, "Deposit");

        line.Reconcile(null);

        line.IsReconciled.Should().BeTrue();
        line.ReconciledAt.Should().NotBeNull();
        line.MatchedJournalEntryId.Should().BeNull();
    }

    [Fact]
    public void Unreconcile_AfterReconcile_ResetsAllThreeFields()
    {
        var line = BankStatementLine.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 100m, "Deposit");
        line.Reconcile(Guid.NewGuid());

        line.Unreconcile();

        line.IsReconciled.Should().BeFalse();
        line.ReconciledAt.Should().BeNull();
        line.MatchedJournalEntryId.Should().BeNull();
    }
}
