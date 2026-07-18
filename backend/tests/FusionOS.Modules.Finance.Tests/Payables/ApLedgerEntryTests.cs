using FluentAssertions;
using FusionOS.Modules.Finance.Domain.Payables;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Payables;

public class ApLedgerEntryTests
{
    [Fact]
    public void RecordBillCharge_WithValidData_SetsAmount()
    {
        var entry = ApLedgerEntry.RecordBillCharge(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 250.75m, "Office supplies bill");

        entry.Amount.Should().Be(250.75m);
    }

    [Fact]
    public void RecordBillCharge_WithNullPurchaseOrderId_Succeeds()
    {
        var entry = ApLedgerEntry.RecordBillCharge(Guid.NewGuid(), Guid.NewGuid(), null, 100m, "Ad-hoc consulting bill");

        entry.PurchaseOrderId.Should().BeNull();
        entry.Amount.Should().Be(100m);
    }

    [Fact]
    public void RecordBillCharge_WithZeroAmount_Throws()
    {
        var act = () => ApLedgerEntry.RecordBillCharge(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m, "Bill");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordBillCharge_WithNegativeAmount_Throws()
    {
        var act = () => ApLedgerEntry.RecordBillCharge(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -10m, "Bill");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordBillCharge_WithEmptySupplierId_Throws()
    {
        var act = () => ApLedgerEntry.RecordBillCharge(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 100m, "Bill");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordBillCharge_WithEmptyDescription_Throws()
    {
        var act = () => ApLedgerEntry.RecordBillCharge(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, "   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordPayment_WithValidData_SetsNegativeAmount()
    {
        var entry = ApLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, reference: null);

        entry.Amount.Should().Be(-100m);
    }

    [Fact]
    public void RecordPayment_WithNullPurchaseOrderId_Succeeds()
    {
        var entry = ApLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), null, 100m, reference: null);

        entry.PurchaseOrderId.Should().BeNull();
        entry.Amount.Should().Be(-100m);
    }

    [Fact]
    public void RecordPayment_WithReference_IncludesItInDescription()
    {
        var entry = ApLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, reference: "WIRE12345");

        entry.Description.Should().Contain("WIRE12345");
    }

    [Fact]
    public void RecordPayment_WithoutReference_StillProducesADescription()
    {
        var entry = ApLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, reference: null);

        entry.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RecordPayment_WithExplicitTransactionDate_UsesIt()
    {
        var paymentDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var entry = ApLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, reference: null, transactionDate: paymentDate);

        entry.TransactionDate.Should().Be(paymentDate);
    }

    [Fact]
    public void RecordPayment_WithZeroAmount_Throws()
    {
        var act = () => ApLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m, reference: null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordPayment_WithNegativeAmount_Throws()
    {
        var act = () => ApLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -10m, reference: null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordPayment_WithEmptySupplierId_Throws()
    {
        var act = () => ApLedgerEntry.RecordPayment(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 100m, reference: null);

        act.Should().Throw<ArgumentException>();
    }
}
