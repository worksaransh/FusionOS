using FluentValidation;
using FusionOS.Modules.Core.Domain.Comments;

namespace FusionOS.Modules.Core.Application.Comments.Commands.CreateComment;

public sealed class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(Comment.MaxBodyLength);
    }
}
