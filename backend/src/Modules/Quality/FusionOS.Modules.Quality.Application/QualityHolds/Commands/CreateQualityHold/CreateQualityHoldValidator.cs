using FluentValidation;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Commands.CreateQualityHold;

public sealed class CreateQualityHoldValidator : AbstractValidator<CreateQualityHoldCommand>
{
    public CreateQualityHoldValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.InspectionId).NotEqual(Guid.Empty).When(x => x.InspectionId.HasValue)
            .WithMessage("Inspection id, when supplied, cannot be empty.");
    }
}
