using System.Text.Json;
using FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Domain.Payables;
using FusionOS.SharedKernel.Events;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.IntegrationEvents.Consumers;

/// <summary>Covers PurchaseOrderApprovedConsumer — the ordered-amount leg of the three-way match's PurchaseOrderFact (2026-07-20).</summary>
public class PurchaseOrderApprovedConsumerTests
{
    private static string BuildPayload(Guid purchaseOrderId, Guid companyId, Guid supplierId, decimal totalAmount) =>
        JsonSerializer.Serialize(new
        {
            PurchaseOrderId = purchaseOrderId,
            CompanyId = companyId,
            SupplierId = supplierId,
            TotalAmount = totalAmount,
        });

    [Fact]
    public async Task HandleAsync_WhenNoFactExistsYet_CreatesOneFromTheApproval()
    {
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var companyId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        factRepository.GetByPurchaseOrderIdAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns((PurchaseOrderFact?)null);
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderApprovedConsumer(factRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(purchaseOrderId, companyId, Guid.NewGuid(), 1200m);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        await factRepository.Received(1).AddAsync(
            Arg.Is<PurchaseOrderFact>(f => f.PurchaseOrderId == purchaseOrderId && f.OrderedAmount == 1200m && f.ReceivedAmount == 0m),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAGoodsReceiptFactAlreadyExists_FillsInTheOrderedAmount()
    {
        // The costed-receipt event beat the approval event here (no cross-topic
        // ordering guarantee) — the fact already exists with OrderedAmount null.
        var companyId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var existingFact = PurchaseOrderFact.FromGoodsReceipt(companyId, purchaseOrderId, supplierId, 300m);
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        factRepository.GetByPurchaseOrderIdAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(existingFact);
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderApprovedConsumer(factRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(purchaseOrderId, companyId, supplierId, 1200m);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        Assert.Equal(1200m, existingFact.OrderedAmount);
        Assert.Equal(300m, existingFact.ReceivedAmount);
        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenEventAlreadyProcessed_DoesNothing()
    {
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderApprovedConsumer(factRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // Malformed-but-parseable payload guard (2026-07-21): System.Text.Json
    // silently defaults a missing/mistyped property to default(T) instead of
    // throwing, so an event missing PurchaseOrderId/CompanyId/SupplierId would
    // otherwise create a PurchaseOrderFact keyed on Guid.Empty with no error.
    // These payloads are shaped as literal JSON (not via BuildPayload) so a
    // property really is absent from the wire JSON, matching how the bug
    // would actually manifest.

    [Fact]
    public async Task HandleAsync_WhenPurchaseOrderIdIsMissing_ThrowsAndDoesNotPersist()
    {
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderApprovedConsumer(factRepository, processedEvents, unitOfWork);
        var payload = JsonSerializer.Serialize(new { CompanyId = Guid.NewGuid(), SupplierId = Guid.NewGuid(), TotalAmount = 500m });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None));

        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyIdIsEmpty_ThrowsAndDoesNotPersist()
    {
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderApprovedConsumer(factRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 500m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None));

        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSupplierIdIsEmpty_ThrowsAndDoesNotPersist()
    {
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderApprovedConsumer(factRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 500m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None));

        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTotalAmountIsNegative_ThrowsAndDoesNotPersist()
    {
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderApprovedConsumer(factRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -1m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None));

        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
