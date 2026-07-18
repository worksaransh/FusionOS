using FluentValidation;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Commands.CreatePluginListing;

public sealed class CreatePluginListingValidator : AbstractValidator<CreatePluginListingCommand>
{
    public CreatePluginListingValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Publisher).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).IsInEnum();
    }
}
