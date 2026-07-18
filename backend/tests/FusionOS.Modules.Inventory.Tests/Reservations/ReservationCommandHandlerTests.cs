using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Reservations.Commands.CreateReservation;
using FusionOS.Modules.Inventory.Application.Reservations.Commands.FulfillReservation;
using FusionOS.Modules.Inventory.Application.Reservations.Commands.ReleaseReservation;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;
using FusionOS.Modules.Inventory.Domain.Reservations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Reservations;

public class ReservationCommandHandlerTests
{
    [Fact]
    public async Task CreateReservation_PersistsActiveReservation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IReservationRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateReservationCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateReservationCommand(companyId, Guid.NewGuid(), Guid.NewGuid(), 10m, "SalesOrderLine", Guid.NewGuid()),
            CancellationToken.None);

        result.Status.Should().Be("Active");
        await repository.Received(1).AddAsync(Arg.Any<Reservation>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReleaseReservation_ResolvesToReleased()
    {
        var companyId = Guid.NewGuid();
        var reservation = Reservation.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), 10m, "SalesOrderLine", Guid.NewGuid());
        var repository = Substitute.For<IReservationRepository>();
        repository.GetByIdAsync(companyId, reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        var handler = new ReleaseReservationCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new ReleaseReservationCommand(companyId, reservation.Id), CancellationToken.None);

        result.Status.Should().Be("Released");
    }

    [Fact]
    public async Task FulfillReservation_ResolvesToFulfilled()
    {
        var companyId = Guid.NewGuid();
        var reservation = Reservation.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), 10m, "SalesOrderLine", Guid.NewGuid());
        var repository = Substitute.For<IReservationRepository>();
        repository.GetByIdAsync(companyId, reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        var handler = new FulfillReservationCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new FulfillReservationCommand(companyId, reservation.Id), CancellationToken.None);

        result.Status.Should().Be("Fulfilled");
    }

    [Fact]
    public async Task ReleaseReservation_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IReservationRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Reservation?)null);
        var handler = new ReleaseReservationCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new ReleaseReservationCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
