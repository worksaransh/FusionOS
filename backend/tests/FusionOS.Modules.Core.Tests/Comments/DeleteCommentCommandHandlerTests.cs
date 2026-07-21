using FusionOS.Modules.Core.Application.Comments.Commands.DeleteComment;
using FusionOS.Modules.Core.Application.Comments.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Comments;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Comments;

/// <summary>Covers the author-or-moderator delete override — the one piece of authorization logic this feature needed beyond the blanket IRequirePermission gate.</summary>
public class DeleteCommentCommandHandlerTests
{
    private static (ICommentRepository Comments, ICurrentUserContext CurrentUser, IUnitOfWork UnitOfWork, DeleteCommentCommandHandler Handler)
        BuildHandler(Comment comment, Guid actingUserId, bool hasModeratorPermission)
    {
        var comments = Substitute.For<ICommentRepository>();
        comments.GetByIdAsync(comment.CompanyId, comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(actingUserId);
        currentUser.HasPermission("core.comment.delete").Returns(hasModeratorPermission);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteCommentCommandHandler(comments, currentUser, unitOfWork);
        return (comments, currentUser, unitOfWork, handler);
    }

    [Fact]
    public async Task Handle_ByTheOwningAuthorWithoutTheModerationPermission_DeletesTheComment()
    {
        var authorId = Guid.NewGuid();
        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), authorId, "Body");
        var (comments, _, unitOfWork, handler) = BuildHandler(comment, authorId, hasModeratorPermission: false);

        await handler.Handle(new DeleteCommentCommand(comment.CompanyId, comment.Id), CancellationToken.None);

        await comments.Received(1).RemoveAsync(comment, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ByAModeratorWhoIsNotTheAuthor_DeletesTheComment()
    {
        var authorId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), authorId, "Body");
        var (comments, _, unitOfWork, handler) = BuildHandler(comment, moderatorId, hasModeratorPermission: true);

        await handler.Handle(new DeleteCommentCommand(comment.CompanyId, comment.Id), CancellationToken.None);

        await comments.Received(1).RemoveAsync(comment, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ByAnUnrelatedUserWithoutTheModerationPermission_ThrowsAndDoesNotDelete()
    {
        var authorId = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();
        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), authorId, "Body");
        var (comments, _, unitOfWork, handler) = BuildHandler(comment, someoneElse, hasModeratorPermission: false);

        var act = () => handler.Handle(new DeleteCommentCommand(comment.CompanyId, comment.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await comments.DidNotReceive().RemoveAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
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
        var handler = new DeleteCommentCommandHandler(comments, currentUser, unitOfWork);

        var act = () => handler.Handle(new DeleteCommentCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
