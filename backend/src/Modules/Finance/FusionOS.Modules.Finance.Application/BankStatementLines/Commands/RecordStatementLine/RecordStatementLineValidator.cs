using FluentValidation;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.RecordStatementLine;

public sealed class RecordStatementLineValidator : AbstractValidator<RecordStatementLineCommand>
{
    public RecordStatementLineValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BankAccountId).NotEmpty();
        RuleFor(x => x.Amount).NotEqual(0m).WithMessage("A statement line amount cannot be zero.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
