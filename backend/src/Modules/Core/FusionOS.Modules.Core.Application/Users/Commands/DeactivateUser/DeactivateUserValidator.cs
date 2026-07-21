using FluentValidation;

namespace FusionOS.Modules.Core.Application.Users.Commands.DeactivateUser;

public sealed class DeactivateUserValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
