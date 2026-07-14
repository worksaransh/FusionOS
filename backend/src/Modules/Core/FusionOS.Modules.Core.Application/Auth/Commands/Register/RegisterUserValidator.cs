using FluentValidation;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Register;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(10)
            .WithMessage("Password must be at least 10 characters.");
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}
