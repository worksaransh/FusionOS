using System.Text.Json;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.Modules.Core.Domain.Workflow;
using FusionOS.SharedKernel.Events;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.IntegrationEvents.Consumers;

public class PurchaseOrderSubmittedForApprovalConsumerTests
{
    private const string ApproverPermissionCode = "procurement.purchase-order.approve";

    private static string BuildPayload(Guid purchaseOrderId, Guid companyId, Guid requestedByUserId, decimal amount, string? description = "Some PO") =>
        JsonSerializer.Serialize(new
        {
            PurchaseOrderId = purchaseOrderId,
            CompanyId = companyId,
            RequestedByUserId = requestedByUserId,
            Amount = amount,
            Description = description,
        });

    [Fact]
    public async Task HandleAsync_WhenApproversExist_CreatesApprovalRequestAndNotifiesFirstApprover()
    {
        var companyId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var approvalRequests = Substitute.For<IApprovalRequestRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        var users = Substitute.For<IUserRepository>();
        users.ListUserIdsWithPermissionAsync(companyId, ApproverPermissionCode, Arg.Any<CancellationToken>())
            .Returns(new[] { approverId });
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderSubmittedForApprovalConsumer(approvalRequests, notifications, users, processedEvents, unitOfWork);
        var payload = BuildPayload(purchaseOrderId, companyId, requestedBy, 1200m);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        await approvalRequests.Received(1).AddAsync(
            Arg.Is<ApprovalRequest>(r => r.EntityType == "PurchaseOrder" && r.EntityId == purchaseOrderId && r.RequestedBy == requestedBy),
            Arg.Any<CancellationToken>());
        await notifications.Received(1).AddAsync(
            Arg.Is<Notification>(n => n.RecipientUserId == approverId),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoApproversExist_MarksProcessedButCreatesNoApprovalRequest()
    {
        var companyId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();

        var approvalRequests = Substitute.For<IApprovalRequestRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        var users = Substitute.For<IUserRepository>();
        users.ListUserIdsWithPermissionAsync(companyId, ApproverPermissionCode, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Guid>());
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderSubmittedForApprovalConsumer(approvalRequests, notifications, users, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), companyId, requestedBy, 1200m);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        await approvalRequests.DidNotReceive().AddAsync(Arg.Any<ApprovalRequest>(), Arg.Any<CancellationToken>());
        await notifications.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheOnlyApproverIsTheRequester_ExcludesThemAndCreatesNoApprovalRequest()
    {
        var companyId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();

        var approvalRequests = Substitute.For<IApprovalRequestRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        var users = Substitute.For<IUserRepository>();
        users.ListUserIdsWithPermissionAsync(companyId, ApproverPermissionCode, Arg.Any<CancellationToken>())
            .Returns(new[] { requestedBy });
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderSubmittedForApprovalConsumer(approvalRequests, notifications, users, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), companyId, requestedBy, 1200m);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        await approvalRequests.DidNotReceive().AddAsync(Arg.Any<ApprovalRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenEventAlreadyProcessed_DoesNothing()
    {
        var approvalRequests = Substitute.For<IApprovalRequestRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        var users = Substitute.For<IUserRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderSubmittedForApprovalConsumer(approvalRequests, notifications, users, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await approvalRequests.DidNotReceive().AddAsync(Arg.Any<ApprovalRequest>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPurchaseOrderIdIsMissing_ThrowsAndDoesNotPersist()
    {
        var approvalRequests = Substitute.For<IApprovalRequestRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        var users = Substitute.For<IUserRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderSubmittedForApprovalConsumer(approvalRequests, notifications, users, processedEvents, unitOfWork);
        var payload = JsonSerializer.Serialize(new { CompanyId = Guid.NewGuid(), RequestedByUserId = Guid.NewGuid(), Amount = 500m });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None));

        await approvalRequests.DidNotReceive().AddAsync(Arg.Any<ApprovalRequest>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
