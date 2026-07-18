using FluentValidation;

namespace FusionOS.Modules.Finance.Application.Accounts.Commands.DeactivateAccount;

public sealed class DeactivateAccountValidator : AbstractValidator<DeactivateAccountCommand>
{
    public DeactivateAccountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
    }
}
