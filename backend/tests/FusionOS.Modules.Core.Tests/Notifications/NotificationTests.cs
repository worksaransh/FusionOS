using FusionOS.Modules.Core.Domain.Notifications;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Notifications;

public class NotificationTests
{
    [Fact]
    public void Create_StartsUnreadAndPendingDelivery()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), "Hi", "Body");

        notification.IsRead.Should().BeFalse();
        notification.DeliveryStatus.Should().Be(NotificationDeliveryStatus.Pending);
        notification.DeliveredAt.Should().BeNull();
        notification.DeliveryError.Should().BeNull();
    }

    [Fact]
    public void MarkRead_SetsIsReadTrue()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), "Hi", "Body");

        notification.MarkRead();

        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    public void MarkDelivered_SetsSentStatusAndClearsAnyPriorError()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), "Hi", "Body");
        notification.MarkDeliveryFailed("temporary failure");

        notification.MarkDelivered();

        notification.DeliveryStatus.Should().Be(NotificationDeliveryStatus.Sent);
        notification.DeliveredAt.Should().NotBeNull();
        notification.DeliveryError.Should().BeNull();
    }

    [Fact]
    public void MarkDeliveryFailed_SetsFailedStatusAndRecordsError()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), "Hi", "Body");

        notification.MarkDeliveryFailed("SendGrid returned 401.");

        notification.DeliveryStatus.Should().Be(NotificationDeliveryStatus.Failed);
        notification.DeliveryError.Should().Be("SendGrid returned 401.");
        notification.DeliveredAt.Should().BeNull();
    }
}
