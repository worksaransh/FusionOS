using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;

public sealed class CreateInspectionValidator : AbstractValidator<CreateInspectionCommand>
{
    public CreateInspectionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ReferenceId).NotEmpty();
        // Characteristics may be empty only when an InspectionPlanId is supplied to default
        // them from — the handler resolves the plan in that case. If no plan is given, at
        // least one explicit characteristic is still required, unchanged from before.
        RuleFor(x => x.Characteristics)
            .NotEmpty()
            .WithMessage("An inspection must check at least one characteristic.")
            .When(x => x.InspectionPlanId is null);
        RuleForEach(x => x.Characteristics).NotEmpty().MaximumLength(200);

        RuleForEach(x => x.ControlLimits).ChildRules(limits =>
        {
            limits.RuleFor(l => l.Characteristic).NotEmpty().MaximumLength(200);
            limits.RuleFor(l => l).Must(l => !l.LowerControlLimit.HasValue || !l.UpperControlLimit.HasValue || l.LowerControlLimit <= l.UpperControlLimit)
                .WithMessage("Lower control limit cannot exceed the upper control limit.");
        });
    }
}
