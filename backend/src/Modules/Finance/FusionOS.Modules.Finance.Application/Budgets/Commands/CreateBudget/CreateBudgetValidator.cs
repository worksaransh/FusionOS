using FluentValidation;

namespace FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;

public sealed class CreateBudgetValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PeriodStart).NotEmpty();
        RuleFor(x => x.PeriodEnd).NotEmpty().GreaterThan(x => x.PeriodStart)
            .WithMessage("Budget period end must be after period start.");
    }
}
