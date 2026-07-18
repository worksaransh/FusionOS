using FluentAssertions;
using FusionOS.Modules.Finance.Domain.Receivables;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Receivables;

public class ArLedgerEntryTests
{
    [Fact]
    public void RecordInvoiceCharge_WithValidData_SetsAmount()
    {
        var entry = ArLedgerEntry.RecordInvoiceCharge(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 250.75m);

        entry.Amount.Should().Be(250.75m);
    }

    [Fact]
    public void RecordInvoiceCharge_WithZeroAmount_Throws()
    {
        var act = () => ArLedgerEntry.RecordInvoiceCharge(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordInvoiceCharge_WithNegativeAmount_Throws()
    {
        var act = () => ArLedgerEntry.RecordInvoiceCharge(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -10m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordInvoiceCharge_WithEmptyCustomerId_Throws()
    {
        var act = () => ArLedgerEntry.RecordInvoiceCharge(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 100m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordPayment_WithValidData_SetsNegativeAmount()
    {
        var entry = ArLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, reference: null);

        entry.Amount.Should().Be(-100m);
    }

    [Fact]
    public void RecordPayment_WithReference_IncludesItInDescription()
    {
        var entry = ArLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, reference: "UTR12345");

        entry.Description.Should().Contain("UTR12345");
    }

    [Fact]
    public void RecordPayment_WithoutReference_StillProducesADescription()
    {
        var entry = ArLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, reference: null);

        entry.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RecordPayment_WithExplicitTransactionDate_UsesIt()
    {
        var paymentDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var entry = ArLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, reference: null, transactionDate: paymentDate);

        entry.TransactionDate.Should().Be(paymentDate);
    }

    [Fact]
    public void RecordPayment_WithZeroAmount_Throws()
    {
        var act = () => ArLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m, reference: null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordPayment_WithNegativeAmount_Throws()
    {
        var act = () => ArLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -10m, reference: null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordPayment_WithEmptyCustomerId_Throws()
    {
        var act = () => ArLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 100m, reference: null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordPayment_WithEmptyInvoiceId_Throws()
    {
        var act = () => ArLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 100m, reference: null);

        act.Should().Throw<ArgumentException>();
    }
}
