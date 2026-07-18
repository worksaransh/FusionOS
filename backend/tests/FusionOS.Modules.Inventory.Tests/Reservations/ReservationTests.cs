using FluentAssertions;
using FusionOS.Modules.Inventory.Domain.Reservations;
using FusionOS.Modules.Inventory.Domain.Reservations.Events;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Reservations;

public class ReservationTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid Reference = Guid.NewGuid();

    private static Reservation New() => Reservation.Create(Company, Product, Warehouse, 10m, "SalesOrderLine", Reference);

    [Fact]
    public void Create_Active_RaisesCreatedEvent()
    {
        var reservation = New();

        reservation.Status.Should().Be(ReservationStatus.Active);
        reservation.DomainEvents.Should().ContainSingle(e => e is ReservationCreated);
    }

    [Fact]
    public void Create_WithZeroQuantity_Throws()
    {
        var act = () => Reservation.Create(Company, Product, Warehouse, 0m, "SalesOrderLine", Reference);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Release_FromActive_Transitions()
    {
        var reservation = New();

        reservation.Release();

        reservation.Status.Should().Be(ReservationStatus.Released);
    }

    [Fact]
    public void Release_WhenNotActive_Throws()
    {
        var reservation = New();
        reservation.Release();

        var act = () => reservation.Release();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fulfill_FromActive_TransitionsAndRaisesFulfilled()
    {
        var reservation = New();

        reservation.Fulfill();

        reservation.Status.Should().Be(ReservationStatus.Fulfilled);
        reservation.DomainEvents.Should().ContainSingle(e => e is ReservationFulfilled);
    }

    [Fact]
    public void Fulfill_WhenNotActive_Throws()
    {
        var reservation = New();
        reservation.Fulfill();

        var act = () => reservation.Fulfill();

        act.Should().Throw<InvalidOperationException>();
    }
}
