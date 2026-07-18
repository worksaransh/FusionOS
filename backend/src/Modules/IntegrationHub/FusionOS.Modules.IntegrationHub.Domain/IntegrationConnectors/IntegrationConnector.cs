using FusionOS.SharedKernel;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors.Events;

namespace FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;

/// <summary>
/// Phase 9 — Integration Hub, first slice: the connector catalog
/// (05_MODULE_ROADMAP.md's IntegrationHub line item — Shopify, WooCommerce,
/// Amazon, Flipkart, ONDC, Shiprocket, Delhivery, Razorpay, Stripe,
/// WhatsApp, Email). Pure master data (Code/Name/Provider/Category/
/// IsActive), same shape as Marketplace's PluginListing — one
/// <see cref="ConnectorCategory"/> enum spans all eleven named providers
/// rather than eleven separate aggregates, since they share the same
/// catalog shape.
///
/// Scoped per-company, same reasoning as PluginListing: a shared,
/// platform-curated catalog every company picks from would need a
/// platform-admin/publisher concept this codebase has no precedent for yet.
/// </summary>
public sealed class IntegrationConnector : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Provider { get; private set; } = default!;
    public ConnectorCategory Category { get; private set; }
    public bool IsActive { get; private set; } = true;

    private IntegrationConnector() { }

    public static IntegrationConnector Create(Guid companyId, string code, string name, string provider, ConnectorCategory category)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Connector code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connector name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required.", nameof(provider));

        var connector = new IntegrationConnector
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Provider = provider.Trim(),
            Category = category,
        };

        connector.Raise(new IntegrationConnectorCreated(connector.Id, companyId, connector.Code));
        return connector;
    }

    /// <summary>Same "soft-deactivate, never hard-delete" convention as every other master-data aggregate in this codebase.</summary>
    public void Deactivate() => IsActive = false;
}
