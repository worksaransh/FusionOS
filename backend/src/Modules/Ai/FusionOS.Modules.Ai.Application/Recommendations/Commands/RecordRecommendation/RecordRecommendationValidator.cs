using FluentValidation;

namespace FusionOS.Modules.Ai.Application.Recommendations.Commands.RecordRecommendation;

public sealed class RecordRecommendationValidator : AbstractValidator<RecordRecommendationCommand>
{
    public RecordRecommendationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ModelVersion).NotEmpty().MaximumLength(50);
    }
}
