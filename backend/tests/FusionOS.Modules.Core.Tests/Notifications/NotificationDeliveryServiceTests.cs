using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Services;
using FusionOS.Modules.Core.Domain.Identity;
using FusionOS.Modules.Core.Domain.Notifications;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Notifications;

/// <summary>Covers NotificationDeliveryService — the testable core behind NotificationDeliveryDispatcher (Phase M7 remaining, 2026-07-16).</summary>
public class NotificationDeliveryServiceTests
{
    [Fact]
    public async Task DeliverPendingAsync_WithNoPendingNotifications_ReturnsZeroAndDoesNotSave()
    {
        var notificationRepository = Substitute.For<INotificationRepository>();
        notificationRepository.GetPendingDeliveryAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Notification>());
        var userRepository = Substitute.For<IUserRepository>();
        var sender = Substitute.For<INotificationSender>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var service = new NotificationDeliveryService(notificationRepository, userRepository, sender, unitOfWork);

        var processed = await service.DeliverPendingAsync(50, CancellationToken.None);

        processed.Should().Be(0);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeliverPendingAsync_WhenSendSucceeds_MarksDeliveredAndSaves()
    {
        var recipientId = Guid.NewGuid();
        var notification = Notification.Create(Guid.NewGuid(), recipientId, "Hi", "Body");
        var user = User.Register("recipient@example.com", "Recipient", "hash");
        var notificationRepository = Substitute.For<INotificationRepository>();
        notificationRepository.GetPendingDeliveryAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Notification> { notification });
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(recipientId, Arg.Any<CancellationToken>()).Returns(user);
        var sender = Substitute.For<INotificationSender>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var service = new NotificationDeliveryService(notificationRepository, userRepository, sender, unitOfWork);

        var processed = await service.DeliverPendingAsync(50, CancellationToken.None);

        processed.Should().Be(1);
        notification.DeliveryStatus.Should().Be(NotificationDeliveryStatus.Sent);
        await sender.Received(1).SendAsync("recipient@example.com", "Hi", "Body", Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeliverPendingAsync_WhenSendThrows_MarksFailedWithTheExceptionMessageAndStillSaves()
    {
        var recipientId = Guid.NewGuid();
        var notification = Notification.Create(Guid.NewGuid(), recipientId, "Hi", "Body");
        var user = User.Register("recipient@example.com", "Recipient", "hash");
        var notificationRepository = Substitute.For<INotificationRepository>();
        notificationRepository.GetPendingDeliveryAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Notification> { notification });
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(recipientId, Arg.Any<CancellationToken>()).Returns(user);
        var sender = Substitute.For<INotificationSender>();
        sender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("SendGrid returned 401.")));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var service = new NotificationDeliveryService(notificationRepository, userRepository, sender, unitOfWork);

        var processed = await service.DeliverPendingAsync(50, CancellationToken.None);

        processed.Should().Be(1);
        notification.DeliveryStatus.Should().Be(NotificationDeliveryStatus.Failed);
        notification.DeliveryError.Should().Be("SendGrid returned 401.");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeliverPendingAsync_WhenRecipientUserIsMissing_MarksFailedWithoutCallingSender()
    {
        var recipientId = Guid.NewGuid();
        var notification = Notification.Create(Guid.NewGuid(), recipientId, "Hi", "Body");
        var notificationRepository = Substitute.For<INotificationRepository>();
        notificationRepository.GetPendingDeliveryAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Notification> { notification });
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(recipientId, Arg.Any<CancellationToken>()).Returns((User?)null);
        var sender = Substitute.For<INotificationSender>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var service = new NotificationDeliveryService(notificationRepository, userRepository, sender, unitOfWork);

        await service.DeliverPendingAsync(50, CancellationToken.None);

        notification.DeliveryStatus.Should().Be(NotificationDeliveryStatus.Failed);
        notification.DeliveryError.Should().Be("Recipient user was not found.");
        await sender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
