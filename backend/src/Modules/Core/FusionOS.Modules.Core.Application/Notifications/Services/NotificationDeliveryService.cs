using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;

namespace FusionOS.Modules.Core.Application.Notifications.Services;

/// <summary>
/// The actual delivery logic behind NotificationDeliveryDispatcher (Phase M7
/// remaining, 2026-07-16). Split out from the BackgroundService wrapper
/// specifically so it can be unit tested with substituted repositories/sender
/// (BackgroundService.ExecuteAsync's infinite loop is not itself worth
/// testing — same restraint as every Kafka consumer in this codebase, where
/// the testable unit is the Handle method, not the hosted-service scaffolding).
/// </summary>
public sealed class NotificationDeliveryService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationSender _sender;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationDeliveryService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        INotificationSender sender,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _sender = sender;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Attempts delivery of up to <paramref name="batchSize"/> pending notifications. Returns the number processed (delivered or failed), for test assertions and dispatcher logging.</summary>
    public async Task<int> DeliverPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var pending = await _notificationRepository.GetPendingDeliveryAsync(batchSize, cancellationToken);
        if (pending.Count == 0)
            return 0;

        foreach (var notification in pending)
        {
            var recipient = await _userRepository.GetByIdAsync(notification.RecipientUserId, cancellationToken);
            if (recipient is null)
            {
                // The recipient User is same-module data (Notification and User both
                // live in Core), so unlike a cross-module reference this is a real
                // integrity check, not an opaque id we choose not to validate.
                notification.MarkDeliveryFailed("Recipient user was not found.");
                continue;
            }

            try
            {
                await _sender.SendAsync(recipient.Email, notification.Title, notification.Body, cancellationToken);
                notification.MarkDelivered();
            }
            catch (Exception ex)
            {
                notification.MarkDeliveryFailed(ex.Message);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return pending.Count;
    }
}
