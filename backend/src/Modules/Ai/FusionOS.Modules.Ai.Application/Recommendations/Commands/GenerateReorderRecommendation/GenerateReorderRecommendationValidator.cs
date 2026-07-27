using FluentValidation;

namespace FusionOS.Modules.Ai.Application.Recommendations.Commands.GenerateReorderRecommendation;

public sealed class GenerateReorderRecommendationValidator : AbstractValidator<GenerateReorderRecommendationCommand>
{
    public GenerateReorderRecommendationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
