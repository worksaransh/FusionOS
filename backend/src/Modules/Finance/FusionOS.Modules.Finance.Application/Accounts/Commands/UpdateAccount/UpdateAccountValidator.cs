using FluentValidation;
using FusionOS.Modules.Finance.Domain.Accounts;

namespace FusionOS.Modules.Finance.Application.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AccountType).NotEmpty().Must(v => Enum.TryParse<AccountType>(v, out _))
            .WithMessage($"AccountType must be one of: {string.Join(", ", Enum.GetNames<AccountType>())}.");
    }
}
