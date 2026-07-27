using FluentValidation;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Commands.SubmitOcrJob;

public sealed class SubmitOcrJobValidator : AbstractValidator<SubmitOcrJobCommand>
{
    public SubmitOcrJobValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.DocumentId).NotEmpty();
    }
}
