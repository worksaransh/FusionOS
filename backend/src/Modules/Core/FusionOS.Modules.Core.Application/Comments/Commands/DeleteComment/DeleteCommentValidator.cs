using FluentValidation;

namespace FusionOS.Modules.Core.Application.Comments.Commands.DeleteComment;

public sealed class DeleteCommentValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CommentId).NotEmpty();
    }
}
