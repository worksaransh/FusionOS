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
}
