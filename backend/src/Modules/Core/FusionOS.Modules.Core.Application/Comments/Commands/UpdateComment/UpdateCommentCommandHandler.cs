using FusionOS.Modules.Core.Application.Comments.Commands.CreateComment;
using FusionOS.Modules.Core.Application.Comments.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Comments.Commands.UpdateComment;

public sealed class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly ICommentRepository _comments;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommentCommandHandler(ICommentRepository comments, ICurrentUserContext currentUser, IUnitOfWork unitOfWork)
    {
        _comments = comments;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var actingUserId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var comment = await _comments.GetByIdAsync(request.CompanyId, request.CommentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Comment {request.CommentId} not found.");

        // Data-dependent authorization the IRequirePermission pipeline can't
        // express by itself — same pattern as MarkNotificationReadCommandHandler's
        // "you can only mark your own notifications as read" check.
        if (comment.AuthorUserId != actingUserId)
            throw new InvalidOperationException("You can only edit your own comments.");

        comment.UpdateBody(request.Body);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateCommentCommandHandler.MapToDto(comment);
    }
}
