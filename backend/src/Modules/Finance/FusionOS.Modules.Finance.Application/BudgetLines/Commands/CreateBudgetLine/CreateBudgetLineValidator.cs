using FluentValidation;

namespace FusionOS.Modules.Finance.Application.BudgetLines.Commands.CreateBudgetLine;

public sealed class CreateBudgetLineValidator : AbstractValidator<CreateBudgetLineCommand>
{
    public CreateBudgetLineValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BudgetId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.BudgetedAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
