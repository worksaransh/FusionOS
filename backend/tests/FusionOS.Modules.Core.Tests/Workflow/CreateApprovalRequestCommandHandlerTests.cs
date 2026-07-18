using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Commands.CreateApprovalRequest;
using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.Modules.Core.Domain.Workflow;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Workflow;

public class CreateApprovalRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_PersistsRequestAndNotifiesFirstApprover()
    {
        var companyId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var approvalRequests = Substitute.For<IApprovalRequestRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(requestedBy);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateApprovalRequestCommandHandler(approvalRequests, notifications, currentUser, unitOfWork);

        var command = new CreateApprovalRequestCommand(companyId, "PurchaseOrder", entityId, new[] { approverId });

        var result = await handler.Handle(command, CancellationToken.None);

        result.EntityType.Should().Be("PurchaseOrder");
        result.RequestedBy.Should().Be(requestedBy);
        result.Status.Should().Be(nameof(ApprovalStatus.Pending));
        await approvalRequests.Received(1).AddAsync(Arg.Any<ApprovalRequest>(), Arg.Any<CancellationToken>());
        await notifications.Received(1).AddAsync(
            Arg.Is<Notification>(n => n.RecipientUserId == approverId),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_Throws()
    {
        var approvalRequests = Substitute.For<IApprovalRequestRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns((Guid?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateApprovalRequestCommandHandler(approvalRequests, notifications, currentUser, unitOfWork);

        var command = new CreateApprovalRequestCommand(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), new[] { Guid.NewGuid() });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
