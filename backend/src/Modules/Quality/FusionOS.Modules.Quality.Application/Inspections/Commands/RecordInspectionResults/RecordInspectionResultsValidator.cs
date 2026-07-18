using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.RecordInspectionResults;

public sealed class RecordInspectionResultsValidator : AbstractValidator<RecordInspectionResultsCommand>
{
    public RecordInspectionResultsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.InspectionId).NotEmpty();
        RuleFor(x => x.Results).NotEmpty().WithMessage("At least one result is required.");
        RuleForEach(x => x.Results).ChildRules(result =>
        {
            result.RuleFor(r => r.Characteristic).NotEmpty().MaximumLength(200);
        });
    }
}
