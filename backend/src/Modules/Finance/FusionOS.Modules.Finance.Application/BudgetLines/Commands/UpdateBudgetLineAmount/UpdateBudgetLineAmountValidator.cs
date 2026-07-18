using FluentValidation;

namespace FusionOS.Modules.Finance.Application.BudgetLines.Commands.UpdateBudgetLineAmount;

public sealed class UpdateBudgetLineAmountValidator : AbstractValidator<UpdateBudgetLineAmountCommand>
{
    public UpdateBudgetLineAmountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BudgetLineId).NotEmpty();
        RuleFor(x => x.BudgetedAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
