using System.Text.Json;
using FusionOS.Modules.Inventory.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.SharedKernel.Events;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.IntegrationEvents.Consumers;

public class CycleCountVarianceRecordedConsumerTests
{
    private static string BuildPayload(Guid cycleCountId, Guid companyId, Guid productId, Guid warehouseId, decimal varianceQuantity) =>
        JsonSerializer.Serialize(new
        {
            CycleCountId = cycleCountId,
            CompanyId = companyId,
            ProductId = productId,
            WarehouseId = warehouseId,
            VarianceQuantity = varianceQuantity,
        });

    [Fact]
    public async Task HandleAsync_WhenNotYetProcessed_PostsLedgerAdjustmentAndMarksProcessed()
    {
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new CycleCountVarianceRecordedConsumer(ledgerRepository, processedEvents, unitOfWork);

        var eventId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var cycleCountId = Guid.NewGuid();
        var payload = BuildPayload(cycleCountId, companyId, productId, warehouseId, -8m);

        await consumer.HandleAsync(eventId, companyId, payload, CancellationToken.None);

        await ledgerRepository.Received(1).AddAsync(
            Arg.Is<InventoryLedgerEntry>(e => e.ProductId == productId && e.WarehouseId == warehouseId && e.QuantityDelta == -8m),
            Arg.Any<CancellationToken>());
        processedEvents.Received(1).MarkProcessed(eventId, "CycleCountVarianceRecorded");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyProcessed_DoesNothing()
    {
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new CycleCountVarianceRecordedConsumer(ledgerRepository, processedEvents, unitOfWork);

        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -8m);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await ledgerRepository.DidNotReceive().AddAsync(Arg.Any<InventoryLedgerEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
