using System.Text.Json;
using FusionOS.Modules.Procurement.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.SharedKernel.Events;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.IntegrationEvents.Consumers;

public class ApprovalRequestRejectedConsumerTests
{
    private static string BuildPayload(Guid approvalRequestId, Guid companyId, string entityType, Guid entityId, Guid requestedBy, int rejectedAtStepNumber) =>
        JsonSerializer.Serialize(new
        {
            ApprovalRequestId = approvalRequestId,
            CompanyId = companyId,
            EntityType = entityType,
            EntityId = entityId,
            RequestedBy = requestedBy,
            RejectedAtStepNumber = rejectedAtStepNumber,
        });

    [Fact]
    public async Task HandleAsync_ForPendingApprovalPurchaseOrder_RejectsIt()
    {
        var companyId = Guid.NewGuid();
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });
        order.SubmitForApproval(Guid.NewGuid());
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new ApprovalRequestRejectedConsumer(repository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), companyId, "PurchaseOrder", order.Id, Guid.NewGuid(), 1);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        order.Status.Should().Be(PurchaseOrderStatus.Rejected);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ForNonPurchaseOrderEntityType_IsANoOp()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new ApprovalRequestRejectedConsumer(repository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), companyId, "SalesOrder", Guid.NewGuid(), Guid.NewGuid(), 1);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        await repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenEventAlreadyProcessed_DoesNothing()
    {
        var repository = Substitute.For<IPurchaseOrderRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new ApprovalRequestRejectedConsumer(repository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), 1);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPayloadCannotBeDeserialized_Throws()
    {
        var repository = Substitute.For<IPurchaseOrderRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new ApprovalRequestRejectedConsumer(repository, processedEvents, unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), "null", CancellationToken.None));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
