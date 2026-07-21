using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.CreateAccount;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Industry).MaximumLength(100);
        RuleFor(x => x.Website).MaximumLength(200);
    }
}
