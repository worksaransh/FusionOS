namespace FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;

/// <summary>
/// The five families covering the eleven named connectors in
/// 05_MODULE_ROADMAP.md's IntegrationHub line item: Ecommerce (Shopify,
/// WooCommerce, Amazon, Flipkart, ONDC), Shipping (Shiprocket, Delhivery),
/// Payment (Razorpay, Stripe), Messaging (WhatsApp), Email. Stored as text
/// via EF value conversion, never a native PostgreSQL enum
/// (04_DATABASE_GUIDELINES.md §10). The specific provider name itself
/// (e.g. "Shopify") is a free-form <see cref="IntegrationConnector.Provider"/>
/// field, not its own enum — hardcoding eleven values now would be brittle
/// against the real list changing.
/// </summary>
public enum ConnectorCategory
{
    Ecommerce,
    Shipping,
    Payment,
    Messaging,
    Email,
}
