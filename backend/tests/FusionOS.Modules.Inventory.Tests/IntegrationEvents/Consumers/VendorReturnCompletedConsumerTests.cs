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

public class VendorReturnCompletedConsumerTests
{
    private static string BuildPayload(Guid vendorReturnId, Guid companyId, Guid purchaseOrderId, Guid productId, Guid warehouseId, decimal quantity, string reason) =>
        JsonSerializer.Serialize(new
        {
            VendorReturnId = vendorReturnId,
            CompanyId = companyId,
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            Reason = reason,
        });

    [Fact]
    public async Task HandleAsync_PostsNegativeLedgerEntry()
    {
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new VendorReturnCompletedConsumer(ledgerRepository, processedEvents, unitOfWork);

        var eventId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var payload = BuildPayload(Guid.NewGuid(), companyId, Guid.NewGuid(), productId, warehouseId, 5m, "Damaged in transit");

        await consumer.HandleAsync(eventId, companyId, payload, CancellationToken.None);

        await ledgerRepository.Received(1).AddAsync(
            Arg.Is<InventoryLedgerEntry>(e => e.ProductId == productId && e.WarehouseId == warehouseId && e.QuantityDelta == -5m),
            Arg.Any<CancellationToken>());
        processedEvents.Received(1).MarkProcessed(eventId, "VendorReturnCompleted");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyProcessed_DoesNothing()
    {
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new VendorReturnCompletedConsumer(ledgerRepository, processedEvents, unitOfWork);

        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, "Damaged");

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await ledgerRepository.DidNotReceive().AddAsync(Arg.Any<InventoryLedgerEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
