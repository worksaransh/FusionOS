using FluentValidation;

namespace FusionOS.Modules.Billing.Application.Plans.Commands.CreatePlan;

public sealed class CreatePlanValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Tier).NotEmpty();
        RuleFor(x => x.MonthlyPricePerUser).GreaterThanOrEqualTo(0);
        RuleFor(x => x.IncludedModules).NotNull();
    }
}
