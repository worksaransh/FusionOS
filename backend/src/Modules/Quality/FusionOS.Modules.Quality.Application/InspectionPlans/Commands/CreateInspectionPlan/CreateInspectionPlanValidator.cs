using FluentValidation;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Commands.CreateInspectionPlan;

public sealed class CreateInspectionPlanValidator : AbstractValidator<CreateInspectionPlanCommand>
{
    public CreateInspectionPlanValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CharacteristicNames).NotEmpty().WithMessage("An inspection plan must define at least one characteristic.");
        RuleForEach(x => x.CharacteristicNames).NotEmpty().MaximumLength(200);
    }
}
