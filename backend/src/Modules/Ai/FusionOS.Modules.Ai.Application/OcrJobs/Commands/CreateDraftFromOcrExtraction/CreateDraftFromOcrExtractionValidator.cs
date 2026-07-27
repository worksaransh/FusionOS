using FluentValidation;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Commands.CreateDraftFromOcrExtraction;

public sealed class CreateDraftFromOcrExtractionValidator : AbstractValidator<CreateDraftFromOcrExtractionCommand>
{
    public CreateDraftFromOcrExtractionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.OcrJobId).NotEmpty();
        RuleFor(x => x.TargetReferenceType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetReferenceId).NotEmpty();
    }
}
