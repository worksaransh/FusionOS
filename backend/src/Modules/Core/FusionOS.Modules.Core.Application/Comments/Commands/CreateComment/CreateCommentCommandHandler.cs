using FusionOS.Modules.Core.Application.Comments.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Comments.Commands.CreateComment;

public sealed class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly ICommentRepository _comments;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCommentCommandHandler(ICommentRepository comments, ICurrentUserContext currentUser, IUnitOfWork unitOfWork)
    {
        _comments = comments;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var authorUserId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var comment = Domain.Comments.Comment.Post(request.CompanyId, request.EntityType, request.EntityId, authorUserId, request.Body);
        await _comments.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(comment);
    }

    internal static CommentDto MapToDto(Domain.Comments.Comment comment) => new(
        comment.Id, comment.EntityType, comment.EntityId, comment.Body, comment.AuthorUserId, comment.CreatedAt, comment.UpdatedAt);
}
