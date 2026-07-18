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

public class WorkOrderCompletedConsumerTests
{
    private static string BuildPayload(Guid workOrderId, Guid companyId, Guid warehouseId, Guid productId, decimal produced, params (Guid ProductId, decimal Qty)[] components) =>
        JsonSerializer.Serialize(new
        {
            WorkOrderId = workOrderId,
            CompanyId = companyId,
            WarehouseId = warehouseId,
            ProductId = productId,
            QuantityProduced = produced,
            Components = components.Select(c => new { ComponentProductId = c.ProductId, QuantityConsumed = c.Qty }).ToArray(),
        });

    [Fact]
    public async Task HandleAsync_PostsNegativeComponentAndPositiveProductLedgerEntries()
    {
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new WorkOrderCompletedConsumer(ledgerRepository, processedEvents, unitOfWork);

        var eventId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var componentA = Guid.NewGuid();
        var payload = BuildPayload(Guid.NewGuid(), companyId, warehouseId, productId, 10m, (componentA, 20m));

        await consumer.HandleAsync(eventId, companyId, payload, CancellationToken.None);

        // One negative adjustment for the consumed component.
        await ledgerRepository.Received(1).AddAsync(
            Arg.Is<InventoryLedgerEntry>(e => e.ProductId == componentA && e.WarehouseId == warehouseId && e.QuantityDelta == -20m),
            Arg.Any<CancellationToken>());
        // One positive adjustment for the produced parent product.
        await ledgerRepository.Received(1).AddAsync(
            Arg.Is<InventoryLedgerEntry>(e => e.ProductId == productId && e.WarehouseId == warehouseId && e.QuantityDelta == 10m),
            Arg.Any<CancellationToken>());
        processedEvents.Received(1).MarkProcessed(eventId, "WorkOrderCompleted");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyProcessed_DoesNothing()
    {
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new WorkOrderCompletedConsumer(ledgerRepository, processedEvents, unitOfWork);

        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, (Guid.NewGuid(), 5m));

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await ledgerRepository.DidNotReceive().AddAsync(Arg.Any<InventoryLedgerEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
