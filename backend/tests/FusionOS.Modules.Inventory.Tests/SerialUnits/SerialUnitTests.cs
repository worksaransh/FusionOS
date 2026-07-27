using FluentAssertions;
using FusionOS.Modules.Inventory.Domain.SerialUnits;
using FusionOS.Modules.Inventory.Domain.SerialUnits.Events;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.SerialUnits;

public class SerialUnitTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();

    private static SerialUnit New() => SerialUnit.Create(Company, Product, "SN-001");

    [Fact]
    public void Create_StartsInStock_AndRaisesRegisteredEvent()
    {
        var unit = New();

        unit.Status.Should().Be(SerialUnitStatus.InStock);
        unit.DomainEvents.Should().ContainSingle(e => e is SerialUnitRegistered);
    }

    [Fact]
    public void Create_WithEmptyProductId_Throws()
    {
        var act = () => SerialUnit.Create(Company, Guid.Empty, "SN-001");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithBlankSerialNumber_Throws()
    {
        var act = () => SerialUnit.Create(Company, Product, " ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkReserved_FromInStock_Transitions()
    {
        var unit = New();

        unit.MarkReserved();

        unit.Status.Should().Be(SerialUnitStatus.Reserved);
    }

    [Fact]
    public void MarkReserved_WhenNotInStock_Throws()
    {
        var unit = New();
        unit.MarkReserved();

        var act = () => unit.MarkReserved();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkSold_FromInStock_Transitions()
    {
        var unit = New();

        unit.MarkSold();

        unit.Status.Should().Be(SerialUnitStatus.Sold);
    }

    [Fact]
    public void MarkSold_FromReserved_Transitions()
    {
        var unit = New();
        unit.MarkReserved();

        unit.MarkSold();

        unit.Status.Should().Be(SerialUnitStatus.Sold);
    }

    [Fact]
    public void MarkSold_WhenAlreadySold_Throws()
    {
        var unit = New();
        unit.MarkSold();

        var act = () => unit.MarkSold();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkReturned_FromSold_Transitions()
    {
        var unit = New();
        unit.MarkSold();

        unit.MarkReturned();

        unit.Status.Should().Be(SerialUnitStatus.Returned);
    }

    [Fact]
    public void MarkReturned_WhenNotSold_Throws()
    {
        var unit = New();

        var act = () => unit.MarkReturned();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkDefective_FromInStock_Transitions()
    {
        var unit = New();

        unit.MarkDefective();

        unit.Status.Should().Be(SerialUnitStatus.Defective);
    }

    [Fact]
    public void MarkDefective_WhenSold_Throws()
    {
        var unit = New();
        unit.MarkSold();

        var act = () => unit.MarkDefective();

        act.Should().Throw<InvalidOperationException>();
    }
}
