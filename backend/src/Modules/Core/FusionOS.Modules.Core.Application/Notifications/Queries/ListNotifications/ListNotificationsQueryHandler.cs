using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Notifications.Queries.ListNotifications;

public sealed class ListNotificationsQueryHandler : IRequestHandler<ListNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserContext _currentUser;

    public ListNotificationsQueryHandler(INotificationRepository repository, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<NotificationDto>> Handle(ListNotificationsQuery request, CancellationToken cancellationToken)
    {
        var recipientUserId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var notifications = await _repository.ListAsync(request.CompanyId, recipientUserId, request.UnreadOnly, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, recipientUserId, request.UnreadOnly, cancellationToken);

        var dtos = notifications.Select(MapToDto).ToList();
        return new PagedResult<NotificationDto>(dtos, request.Page, request.PageSize, total);
    }

    internal static NotificationDto MapToDto(Domain.Notifications.Notification n) => new(n.Id, n.Title, n.Body, n.IsRead, n.CreatedAt, n.DeliveryStatus.ToString());
}
