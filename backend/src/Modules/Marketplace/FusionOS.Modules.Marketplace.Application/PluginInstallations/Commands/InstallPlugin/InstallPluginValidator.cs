using FluentValidation;

namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.InstallPlugin;

public sealed class InstallPluginValidator : AbstractValidator<InstallPluginCommand>
{
    public InstallPluginValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PluginListingId).NotEmpty();
    }
}
