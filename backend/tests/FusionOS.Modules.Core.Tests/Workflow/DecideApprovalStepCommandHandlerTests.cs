using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Commands.DecideApprovalStep;
using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.Modules.Core.Domain.Workflow;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Workflow;

public class DecideApprovalStepCommandHandlerTests
{
    private static (IApprovalRequestRepository Repo, INotificationRepository Notifications, ICurrentUserContext CurrentUser, IUnitOfWork UnitOfWork, DecideApprovalStepCommandHandler Handler)
        BuildHandler(ApprovalRequest request, Guid actingUserId)
    {
        var repo = Substitute.For<IApprovalRequestRepository>();
        repo.GetByIdAsync(request.CompanyId, request.Id, Arg.Any<CancellationToken>()).Returns(request);
        var notifications = Substitute.For<INotificationRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(actingUserId);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DecideApprovalStepCommandHandler(repo, notifications, currentUser, unitOfWork);
        return (repo, notifications, currentUser, unitOfWork, handler);
    }

    [Fact]
    public async Task Handle_ApprovingTheFinalStep_NotifiesTheOriginalRequester()
    {
        var companyId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var request = ApprovalRequest.Submit(companyId, "PurchaseOrder", Guid.NewGuid(), requestedBy, new[] { approverId });
        var (_, notifications, _, unitOfWork, handler) = BuildHandler(request, approverId);

        var result = await handler.Handle(new DecideApprovalStepCommand(companyId, request.Id, true, "OK"), CancellationToken.None);

        result.Status.Should().Be(nameof(ApprovalStatus.Approved));
        await notifications.Received(1).AddAsync(
            Arg.Is<Notification>(n => n.RecipientUserId == requestedBy),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ApprovingAnEarlierStep_NotifiesTheNextApprover()
    {
        var companyId = Guid.NewGuid();
        var approver1 = Guid.NewGuid();
        var approver2 = Guid.NewGuid();
        var request = ApprovalRequest.Submit(companyId, "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { approver1, approver2 });
        var (_, notifications, _, _, handler) = BuildHandler(request, approver1);

        var result = await handler.Handle(new DecideApprovalStepCommand(companyId, request.Id, true, null), CancellationToken.None);

        result.Status.Should().Be(nameof(ApprovalStatus.Pending));
        result.CurrentStepNumber.Should().Be(2);
        await notifications.Received(1).AddAsync(
            Arg.Is<Notification>(n => n.RecipientUserId == approver2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Rejecting_NotifiesTheOriginalRequesterOfTheRejection()
    {
        var companyId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var request = ApprovalRequest.Submit(companyId, "PurchaseOrder", Guid.NewGuid(), requestedBy, new[] { approverId });
        var (_, notifications, _, _, handler) = BuildHandler(request, approverId);

        var result = await handler.Handle(new DecideApprovalStepCommand(companyId, request.Id, false, "No"), CancellationToken.None);

        result.Status.Should().Be(nameof(ApprovalStatus.Rejected));
        await notifications.Received(1).AddAsync(
            Arg.Is<Notification>(n => n.RecipientUserId == requestedBy),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRequestDoesNotExist_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repo = Substitute.For<IApprovalRequestRepository>();
        repo.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ApprovalRequest?)null);
        var notifications = Substitute.For<INotificationRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DecideApprovalStepCommandHandler(repo, notifications, currentUser, unitOfWork);

        var act = () => handler.Handle(new DecideApprovalStepCommand(companyId, Guid.NewGuid(), true, null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenActingUserIsNotTheAssignedApprover_ThrowsAndDoesNotSave()
    {
        var companyId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();
        var request = ApprovalRequest.Submit(companyId, "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { approverId });
        var (_, _, _, unitOfWork, handler) = BuildHandler(request, someoneElse);

        var act = () => handler.Handle(new DecideApprovalStepCommand(companyId, request.Id, true, null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
