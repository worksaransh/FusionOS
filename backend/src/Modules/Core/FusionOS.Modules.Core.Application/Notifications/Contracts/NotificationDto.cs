namespace FusionOS.Modules.Core.Application.Notifications.Contracts;

public sealed record NotificationDto(Guid Id, string Title, string Body, bool IsRead, DateTimeOffset CreatedAt, string DeliveryStatus);
