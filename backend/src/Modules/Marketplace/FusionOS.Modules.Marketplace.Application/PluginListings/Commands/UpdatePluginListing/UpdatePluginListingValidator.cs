using FluentValidation;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Commands.UpdatePluginListing;

public sealed class UpdatePluginListingValidator : AbstractValidator<UpdatePluginListingCommand>
{
    public UpdatePluginListingValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PluginListingId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Publisher).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).IsInEnum();
    }
}
