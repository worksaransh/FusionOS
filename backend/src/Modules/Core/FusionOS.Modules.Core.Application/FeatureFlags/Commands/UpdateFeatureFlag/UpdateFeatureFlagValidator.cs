using FluentValidation;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Commands.UpdateFeatureFlag;

public sealed class UpdateFeatureFlagValidator : AbstractValidator<UpdateFeatureFlagCommand>
{
    public UpdateFeatureFlagValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.FeatureFlagId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.RolloutPercentage).InclusiveBetween(0, 100);
    }
}
