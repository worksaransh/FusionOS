namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;

public sealed record PluginInstallationDto(Guid Id, Guid PluginListingId, string Status, DateTimeOffset InstalledAt, DateTimeOffset? UninstalledAt);

/// <summary>Single place that turns a PluginInstallation aggregate into its DTO, shared by every handler that returns one.</summary>
public static class PluginInstallationMapper
{
    public static PluginInstallationDto ToDto(Domain.PluginInstallations.PluginInstallation installation) =>
        new(installation.Id, installation.PluginListingId, installation.Status.ToString(), installation.InstalledAt, installation.UninstalledAt);
}
