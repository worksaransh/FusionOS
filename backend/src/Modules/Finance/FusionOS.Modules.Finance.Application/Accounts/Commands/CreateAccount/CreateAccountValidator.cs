using FluentValidation;
using FusionOS.Modules.Finance.Domain.Accounts;

namespace FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AccountType).NotEmpty().Must(v => Enum.TryParse<AccountType>(v, out _))
            .WithMessage($"AccountType must be one of: {string.Join(", ", Enum.GetNames<AccountType>())}.");
    }
}
