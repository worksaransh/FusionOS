using FusionOS.SharedKernel;
using FusionOS.Modules.Marketplace.Domain.PluginInstallations.Events;

namespace FusionOS.Modules.Marketplace.Domain.PluginInstallations;

/// <summary>
/// Phase 8 — Marketplace, first slice: a company's install of a
/// PluginListing, Installed → Disabled/Uninstalled. PluginListingId is a
/// same-module FK, existence-validated in the command handler (mirrors
/// CreateBudgetLine/AccountId, CreateMaintenanceRequest/AssetId).
///
/// Deliberately does NOT actually load, sandbox, or execute any plugin code —
/// this record is the install *bookkeeping* only (03_SYSTEM_ARCHITECTURE.md
/// §6 describes an event-subscription + scoped-permission model for real
/// plugin execution that doesn't exist anywhere in this codebase yet;
/// building a fake runtime here would look done in a grep and not be, same
/// restraint as AI's Recommendation not including a real ML model). A real
/// plugin runtime is a separately-scoped, substantially larger follow-up.
/// </summary>
public sealed class PluginInstallation : TenantAggregateRoot
{
    public Guid PluginListingId { get; private set; }
    public InstallationStatus Status { get; private set; }
    public DateTimeOffset InstalledAt { get; private set; }
    public DateTimeOffset? UninstalledAt { get; private set; }

    private PluginInstallation() { }

    public static PluginInstallation Create(Guid companyId, Guid pluginListingId)
    {
        if (pluginListingId == Guid.Empty)
            throw new ArgumentException("Plugin listing id is required.", nameof(pluginListingId));

        var installation = new PluginInstallation
        {
            CompanyId = companyId,
            PluginListingId = pluginListingId,
            Status = InstallationStatus.Installed,
            InstalledAt = DateTimeOffset.UtcNow,
        };

        installation.Raise(new PluginInstalled(installation.Id, companyId, pluginListingId));
        return installation;
    }

    /// <summary>Temporarily turns the installation off without uninstalling — reversible via <see cref="Enable"/>.</summary>
    public void Disable()
    {
        if (Status != InstallationStatus.Installed)
            throw new InvalidOperationException($"Only an Installed plugin can be disabled (current status: {Status}).");

        Status = InstallationStatus.Disabled;
    }

    public void Enable()
    {
        if (Status != InstallationStatus.Disabled)
            throw new InvalidOperationException($"Only a Disabled plugin can be re-enabled (current status: {Status}).");

        Status = InstallationStatus.Installed;
    }

    /// <summary>Terminal — an uninstalled plugin cannot be re-enabled; a fresh Create represents reinstalling it.</summary>
    public void Uninstall()
    {
        if (Status == InstallationStatus.Uninstalled)
            throw new InvalidOperationException("This plugin is already uninstalled.");

        Status = InstallationStatus.Uninstalled;
        UninstalledAt = DateTimeOffset.UtcNow;
    }
}
