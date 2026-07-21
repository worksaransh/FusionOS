using FluentValidation;
using FusionOS.Modules.Core.Domain.Comments;

namespace FusionOS.Modules.Core.Application.Comments.Commands.UpdateComment;

public sealed class UpdateCommentValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(Comment.MaxBodyLength);
    }
}
