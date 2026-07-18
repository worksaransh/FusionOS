using FusionOS.SharedKernel;
using FusionOS.Modules.Marketplace.Domain.PluginListings.Events;

namespace FusionOS.Modules.Marketplace.Domain.PluginListings;

/// <summary>
/// Phase 8 — Marketplace, first slice: the extension catalog
/// (05_MODULE_ROADMAP.md's Marketplace line item — Plugins/Themes/Report
/// packs/Workflow packs/Industry extensions/AI agents, modeled as one
/// <see cref="PluginCategory"/> enum rather than six separate aggregates,
/// since they share the same catalog shape). Pure master data
/// (Code/Name/Publisher/Category/IsActive), same shape as Finance's
/// CostCenter.
///
/// Deliberately scoped per-company (TenantAggregateRoot), not a shared
/// platform-wide public catalog every company browses — a real cross-company
/// marketplace needs a platform-admin/publisher concept (who is allowed to
/// publish a listing visible to every tenant?) this codebase has no
/// precedent for yet, and inventing one here would be a bigger architectural
/// decision than "the first slice" should make. This is honest given there
/// is also no real plugin execution/sandboxing engine yet (see
/// PluginInstallation's own class doc comment) — without one, cross-company
/// listing sharing has no actual mechanism to back it anyway.
/// </summary>
public sealed class PluginListing : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Publisher { get; private set; } = default!;
    public PluginCategory Category { get; private set; }
    public bool IsActive { get; private set; } = true;

    private PluginListing() { }

    public static PluginListing Create(Guid companyId, string code, string name, string publisher, PluginCategory category)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Plugin listing code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plugin listing name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(publisher))
            throw new ArgumentException("Publisher is required.", nameof(publisher));

        var listing = new PluginListing
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Publisher = publisher.Trim(),
            Category = category,
        };

        listing.Raise(new PluginListingCreated(listing.Id, companyId, listing.Code));
        return listing;
    }

    /// <summary>Delists the listing — same "soft-deactivate, never hard-delete" convention as every other master-data aggregate in this codebase.</summary>
    public void Deactivate() => IsActive = false;
}
