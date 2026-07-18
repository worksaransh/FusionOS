using FluentValidation;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;

public sealed class CreateBankAccountValidator : AbstractValidator<CreateBankAccountCommand>
{
    public CreateBankAccountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LinkedAccountId).NotEmpty();
        RuleFor(x => x.BankName).MaximumLength(200);
        RuleFor(x => x.AccountNumberLast4).MaximumLength(4);
    }
}
