using FluentValidation;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Commands.CreateFeatureFlag;

public sealed class CreateFeatureFlagValidator : AbstractValidator<CreateFeatureFlagCommand>
{
    public CreateFeatureFlagValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.RolloutPercentage).InclusiveBetween(0, 100);
    }
}
