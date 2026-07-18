using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Queries.GetApprovalRequest;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.Modules.Core.Domain.Workflow;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Workflow.Commands.DecideApprovalStep;

public sealed class DecideApprovalStepCommandHandler : IRequestHandler<DecideApprovalStepCommand, ApprovalRequestDto>
{
    private readonly IApprovalRequestRepository _approvalRequests;
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DecideApprovalStepCommandHandler(
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

    public async Task<ApprovalRequestDto> Handle(DecideApprovalStepCommand request, CancellationToken cancellationToken)
    {
        var actingUserId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var approvalRequest = await _approvalRequests.GetByIdAsync(request.CompanyId, request.ApprovalRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Approval request {request.ApprovalRequestId} not found.");

        approvalRequest.Decide(actingUserId, request.Approve, request.Comments);

        var notification = approvalRequest.Status switch
        {
            ApprovalStatus.Approved => Notification.Create(
                request.CompanyId,
                approvalRequest.RequestedBy,
                "Approval complete",
                $"Your {approvalRequest.EntityType} approval request was fully approved."),
            ApprovalStatus.Rejected => Notification.Create(
                request.CompanyId,
                approvalRequest.RequestedBy,
                "Approval rejected",
                $"Your {approvalRequest.EntityType} approval request was rejected."),
            _ => Notification.Create(
                request.CompanyId,
                approvalRequest.Steps.Single(s => s.StepNumber == approvalRequest.CurrentStepNumber).ApproverUserId,
                "Approval requested",
                $"You have a pending approval request for {approvalRequest.EntityType} {approvalRequest.EntityId}."),
        };
        await _notifications.AddAsync(notification, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetApprovalRequestQueryHandler.MapToDto(approvalRequest);
    }
}
