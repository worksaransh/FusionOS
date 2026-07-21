using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Industry).MaximumLength(100);
        RuleFor(x => x.Website).MaximumLength(200);
    }
}
