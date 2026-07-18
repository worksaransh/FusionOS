using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Commands.MarkNotificationRead;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Notifications;

public class MarkNotificationReadCommandHandlerTests
{
    [Fact]
    public async Task Handle_ForTheOwningRecipient_MarksItReadAndSaves()
    {
        var companyId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var notification = Notification.Create(companyId, recipientId, "Hi", "Body");
        var repository = Substitute.For<INotificationRepository>();
        repository.GetByIdAsync(companyId, notification.Id, Arg.Any<CancellationToken>()).Returns(notification);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(recipientId);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new MarkNotificationReadCommandHandler(repository, currentUser, unitOfWork);

        var result = await handler.Handle(new MarkNotificationReadCommand(companyId, notification.Id), CancellationToken.None);

        result.IsRead.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ForADifferentUserThanTheRecipient_ThrowsAndDoesNotSave()
    {
        var companyId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();
        var notification = Notification.Create(companyId, recipientId, "Hi", "Body");
        var repository = Substitute.For<INotificationRepository>();
        repository.GetByIdAsync(companyId, notification.Id, Arg.Any<CancellationToken>()).Returns(notification);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(someoneElse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new MarkNotificationReadCommandHandler(repository, currentUser, unitOfWork);

        var act = () => handler.Handle(new MarkNotificationReadCommand(companyId, notification.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        notification.IsRead.Should().BeFalse();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotificationDoesNotExist_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<INotificationRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Notification?)null);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new MarkNotificationReadCommandHandler(repository, currentUser, unitOfWork);

        var act = () => handler.Handle(new MarkNotificationReadCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
