using FusionOS.Modules.Core.Application.Comments.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Comments.Commands.DeleteComment;

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Unit>
{
    private const string ModeratorPermission = "core.comment.delete";

    private readonly ICommentRepository _comments;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(ICommentRepository comments, ICurrentUserContext currentUser, IUnitOfWork unitOfWork)
    {
        _comments = comments;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var actingUserId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var comment = await _comments.GetByIdAsync(request.CompanyId, request.CommentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Comment {request.CommentId} not found.");

        var isOwnComment = comment.AuthorUserId == actingUserId;
        var isModerator = _currentUser.HasPermission(ModeratorPermission);
        if (!isOwnComment && !isModerator)
            throw new InvalidOperationException("You can only delete your own comments unless you hold the comment moderation permission.");

        await _comments.RemoveAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
