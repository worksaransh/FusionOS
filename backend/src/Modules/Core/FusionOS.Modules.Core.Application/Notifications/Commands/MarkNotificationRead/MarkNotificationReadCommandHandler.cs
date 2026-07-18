using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Queries.ListNotifications;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Notifications.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, NotificationDto>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(INotificationRepository repository, ICurrentUserContext currentUser, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationDto> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var actingUserId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var notification = await _repository.GetByIdAsync(request.CompanyId, request.NotificationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Notification {request.NotificationId} not found.");

        // A notification's RecipientUserId is set at creation and never
        // changes, so this is a data-dependent check the IRequirePermission
        // pipeline can't express by itself — same pattern as every other
        // "only the assigned person can act on this" rule in this codebase
        // (PO maker-checker, ApprovalStep's assigned approver).
        if (notification.RecipientUserId != actingUserId)
            throw new InvalidOperationException("You can only mark your own notifications as read.");

        notification.MarkRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ListNotificationsQueryHandler.MapToDto(notification);
    }
}
