using FluentValidation;

namespace FusionOS.Modules.Core.Application.Companies.Commands.SetCompanyParent;

public sealed class SetCompanyParentValidator : AbstractValidator<SetCompanyParentCommand>
{
    public SetCompanyParentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.ParentCompanyId != x.Id)
            .WithMessage("A company cannot be its own parent.");
    }
}
