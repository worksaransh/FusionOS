using FusionOS.Modules.Procurement.Domain.SupplierContracts;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.SupplierContracts;

public class SupplierContractTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidArgs_StartsActive()
    {
        var supplierId = Guid.NewGuid();

        var contract = SupplierContract.Create(Guid.NewGuid(), supplierId, Start, End, "Net 30 payment terms.");

        contract.SupplierId.Should().Be(supplierId);
        contract.Status.Should().Be(SupplierContractStatus.Active);
        contract.Terms.Should().Be("Net 30 payment terms.");
        contract.DomainEvents.Should().ContainSingle(e => e is Events.SupplierContractCreated);
    }

    [Fact]
    public void Create_WithEmptySupplierId_Throws()
    {
        var act = () => SupplierContract.Create(Guid.NewGuid(), Guid.Empty, Start, End, "Terms");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_Throws()
    {
        var act = () => SupplierContract.Create(Guid.NewGuid(), Guid.NewGuid(), End, Start, "Terms");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithBlankTerms_Throws()
    {
        var act = () => SupplierContract.Create(Guid.NewGuid(), Guid.NewGuid(), Start, End, "  ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Terminate_FromActive_SetsStatusAndRaisesEvent()
    {
        var contract = SupplierContract.Create(Guid.NewGuid(), Guid.NewGuid(), Start, End, "Terms");
        contract.ClearDomainEvents();

        contract.Terminate();

        contract.Status.Should().Be(SupplierContractStatus.Terminated);
        contract.DomainEvents.Should().ContainSingle(e => e is Events.SupplierContractTerminated);
    }

    [Fact]
    public void Terminate_WhenAlreadyTerminated_Throws()
    {
        var contract = SupplierContract.Create(Guid.NewGuid(), Guid.NewGuid(), Start, End, "Terms");
        contract.Terminate();

        var act = () => contract.Terminate();

        act.Should().Throw<InvalidOperationException>();
    }
}
