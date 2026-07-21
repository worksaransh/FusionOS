using FusionOS.Modules.Core.Application.Comments.Commands.CreateComment;
using FusionOS.Modules.Core.Application.Comments.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Comments;

public class CreateCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithAnAuthenticatedUser_PersistsTheCommentAuthoredByTheCaller()
    {
        var companyId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var comments = Substitute.For<ICommentRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(authorId);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCommentCommandHandler(comments, currentUser, unitOfWork);

        var command = new CreateCommentCommand(companyId, "PurchaseOrder", entityId, "Please expedite this.");

        var result = await handler.Handle(command, CancellationToken.None);

        result.EntityType.Should().Be("PurchaseOrder");
        result.EntityId.Should().Be(entityId);
        result.AuthorUserId.Should().Be(authorId);
        result.Body.Should().Be("Please expedite this.");
        await comments.Received(1).AddAsync(Arg.Any<Domain.Comments.Comment>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_ThrowsAndDoesNotSave()
    {
        var comments = Substitute.For<ICommentRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns((Guid?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCommentCommandHandler(comments, currentUser, unitOfWork);

        var command = new CreateCommentCommand(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), "Body");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
