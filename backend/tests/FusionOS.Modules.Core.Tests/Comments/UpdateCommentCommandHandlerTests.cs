using FusionOS.Modules.Core.Application.Comments.Commands.UpdateComment;
using FusionOS.Modules.Core.Application.Comments.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Comments;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Comments;

public class UpdateCommentCommandHandlerTests
{
    private static (ICommentRepository Comments, ICurrentUserContext CurrentUser, IUnitOfWork UnitOfWork, UpdateCommentCommandHandler Handler)
        BuildHandler(Comment comment, Guid actingUserId)
    {
        var comments = Substitute.For<ICommentRepository>();
        comments.GetByIdAsync(comment.CompanyId, comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(actingUserId);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateCommentCommandHandler(comments, currentUser, unitOfWork);
        return (comments, currentUser, unitOfWork, handler);
    }

    [Fact]
    public async Task Handle_ByTheOwningAuthor_UpdatesTheBodyAndSaves()
    {
        var authorId = Guid.NewGuid();
        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), authorId, "Original");
        var (_, _, unitOfWork, handler) = BuildHandler(comment, authorId);

        var result = await handler.Handle(new UpdateCommentCommand(comment.CompanyId, comment.Id, "Corrected"), CancellationToken.None);

        result.Body.Should().Be("Corrected");
        comment.Body.Should().Be("Corrected");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ByAUserWhoIsNotTheAuthor_ThrowsAndDoesNotSave()
    {
        var authorId = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();
        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), authorId, "Original");
        var (_, _, unitOfWork, handler) = BuildHandler(comment, someoneElse);

        var act = () => handler.Handle(new UpdateCommentCommand(comment.CompanyId, comment.Id, "Corrected"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        comment.Body.Should().Be("Original");
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var comments = Substitute.For<ICommentRepository>();
        comments.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Comment?)null);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateCommentCommandHandler(comments, currentUser, unitOfWork);

        var act = () => handler.Handle(new UpdateCommentCommand(companyId, Guid.NewGuid(), "New body"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
