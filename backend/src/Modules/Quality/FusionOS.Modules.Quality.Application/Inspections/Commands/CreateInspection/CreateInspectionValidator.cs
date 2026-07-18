using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;

public sealed class CreateInspectionValidator : AbstractValidator<CreateInspectionCommand>
{
    public CreateInspectionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.Characteristics).NotEmpty().WithMessage("An inspection must check at least one characteristic.");
        RuleForEach(x => x.Characteristics).NotEmpty().MaximumLength(200);
    }
}
