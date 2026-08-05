using FluentValidation;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Commands.UpdateInspectionPlan;

public sealed class UpdateInspectionPlanValidator : AbstractValidator<UpdateInspectionPlanCommand>
{
    public UpdateInspectionPlanValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.InspectionPlanId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CharacteristicNames).NotEmpty().WithMessage("An inspection plan must define at least one characteristic.");
        RuleForEach(x => x.CharacteristicNames).NotEmpty().MaximumLength(200);
    }
}
