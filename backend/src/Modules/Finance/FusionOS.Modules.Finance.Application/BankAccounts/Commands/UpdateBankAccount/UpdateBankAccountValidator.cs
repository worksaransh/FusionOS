using FluentValidation;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Commands.UpdateBankAccount;

public sealed class UpdateBankAccountValidator : AbstractValidator<UpdateBankAccountCommand>
{
    public UpdateBankAccountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BankAccountId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BankName).MaximumLength(200);
        RuleFor(x => x.AccountNumberLast4).MaximumLength(4);
    }
}
