using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Queries.GetApprovalRequest;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Workflow.Commands.CreateApprovalRequest;

public sealed class CreateApprovalRequestCommandHandler : IRequestHandler<CreateApprovalRequestCommand, ApprovalRequestDto>
{
    private readonly IApprovalRequestRepository _approvalRequests;
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateApprovalRequestCommandHandler(
        IApprovalRequestRepository approvalRequests,
        INotificationRepository notifications,
        ICurrentUserContext currentUser,
        IUnitOfWork unitOfWork)
    {
        _approvalRequests = approvalRequests;
        _notifications = notifications;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApprovalRequestDto> Handle(CreateApprovalRequestCommand request, CancellationToken cancellationToken)
    {
        var requestedBy = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var approvalRequest = Domain.Workflow.ApprovalRequest.Submit(request.CompanyId, request.EntityType, request.EntityId, requestedBy, request.ApproverUserIds);
        await _approvalRequests.AddAsync(approvalRequest, cancellationToken);

        // Same-transaction, in-process notification — no outbox/consumer round
        // trip needed since Notification lives in this same Core module/DbContext
        // (unlike a real cross-module event, which does need the outbox).
        var firstApproverUserId = approvalRequest.Steps[0].ApproverUserId;
        var notification = Notification.Create(
            request.CompanyId,
            firstApproverUserId,
            "Approval requested",
            $"You have a pending approval request for {request.EntityType} {request.EntityId}.");
        await _notifications.AddAsync(notification, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetApprovalRequestQueryHandler.MapToDto(approvalRequest);
    }
}
