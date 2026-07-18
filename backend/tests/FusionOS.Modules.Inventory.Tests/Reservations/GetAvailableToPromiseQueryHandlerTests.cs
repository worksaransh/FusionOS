using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;
using FusionOS.Modules.Inventory.Application.Reservations.Queries.GetAvailableToPromise;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Reservations;

public class GetAvailableToPromiseQueryHandlerTests
{
    [Fact]
    public async Task Handle_SubtractsActiveReservationsFromStockOnHand()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        ledgerRepository.SumQuantityAsync(companyId, productId, warehouseId, Arg.Any<CancellationToken>()).Returns(100m);
        var reservationRepository = Substitute.For<IReservationRepository>();
        reservationRepository.SumActiveQuantityAsync(companyId, productId, warehouseId, Arg.Any<CancellationToken>()).Returns(30m);
        var handler = new GetAvailableToPromiseQueryHandler(ledgerRepository, reservationRepository);

        var result = await handler.Handle(new GetAvailableToPromiseQuery(companyId, productId, warehouseId), CancellationToken.None);

        result.StockOnHand.Should().Be(100m);
        result.Reserved.Should().Be(30m);
        result.Available.Should().Be(70m);
    }
}
