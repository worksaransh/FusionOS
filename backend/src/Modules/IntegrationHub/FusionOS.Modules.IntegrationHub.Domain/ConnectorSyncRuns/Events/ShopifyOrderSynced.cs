using FusionOS.SharedKernel;

namespace FusionOS.Modules.IntegrationHub.Domain.ConnectorSyncRuns.Events;

/// <summary>
/// Raised once per Shopify order this run pulled from Shopify's REST Admin API
/// (ShopifyConnectorSyncAdapter, polled via TriggerConnectorSyncCommand) or received
/// via the Shopify webhook receiver (WebhooksController, after HMAC-SHA256
/// verification) — either path funnels through <see cref="ConnectorSyncRun.RecordShopifyOrderSynced"/>
/// so the event is always raised by an aggregate, this codebase's one and only
/// convention for producing domain events (AggregateRoot.Raise is only ever called
/// from within an aggregate's own method). Relayed via the outbox to Kafka like any
/// other domain event (03_SYSTEM_ARCHITECTURE.md §4.2) so other modules (e.g. a future
/// Sales/CRM consumer that wants to create a Lead or SalesOrder from an external
/// storefront order) can react without this module taking a compile-time dependency
/// on them — the same "define your own local Payload record" idiom consumers like
/// Finance's InvoiceIssuedConsumer use, just in reverse (the data originates outside
/// FusionOS, not from another FusionOS module's own aggregate).
/// </summary>
public sealed record ShopifyOrderSynced(Guid CompanyId, string ShopifyOrderId, string CustomerEmail, decimal TotalAmount, DateTimeOffset OrderedAt) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
