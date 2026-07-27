namespace FusionOS.Modules.IntegrationHub.Application.Sync;

/// <summary>
/// Phase 8 — AI &amp; Integrations. The shared contract every provider-specific
/// sync adapter (Razorpay, Stripe, Shiprocket, Delhivery, Shopify, Ondc, ...)
/// implements to pull data from an external system into FusionOS.
///
/// UPDATE (2026-07-25): the shared connector-sync framework this interface
/// anticipated has now landed — <c>ConnectorSyncRun</c> (Domain), a
/// <c>TriggerConnectorSyncCommand</c> that resolves the adapter whose
/// <see cref="ProviderName"/> matches the connection's IntegrationConnector.Provider
/// (case-insensitive) and records the result, and
/// <c>ConnectorConnection.CredentialReference</c>. This interface's shape did not
/// need to change to fit that framework — <see cref="ConnectorSyncResult"/> only
/// grew one additive, optional property (<see cref="ConnectorSyncResult.SyncedOrders"/>)
/// so the Razorpay/Stripe/Shiprocket/Delhivery adapters keep compiling unchanged.
/// </summary>
public interface IConnectorSyncAdapter
{
    /// <summary>
    /// The provider name this adapter serves (e.g. "Razorpay"). Matched against
    /// <c>IntegrationConnector.Provider</c> / <c>ConnectorConnection</c>'s
    /// connector to resolve which adapter runs a given connection's sync.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Runs one sync attempt for the given company. <paramref name="credentialReference"/>
    /// is never a secret itself — it is the name under which the real secret(s)
    /// live in <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>
    /// (e.g. an environment-variable name), resolved at call time. Each adapter's
    /// class doc comment documents exactly which config key(s) it derives from
    /// this reference.
    /// </summary>
    Task<ConnectorSyncResult> SyncAsync(Guid companyId, string? credentialReference, CancellationToken cancellationToken);
}

/// <summary>
/// The outcome of one <see cref="IConnectorSyncAdapter.SyncAsync"/> call.
/// <paramref name="RecordsProcessed"/> is the count of external records the
/// adapter successfully fetched and parsed this run. Razorpay/Stripe/Shiprocket/
/// Delhivery do not yet raise FusionOS domain events for each parsed record (e.g.
/// auto-creating a Payment or Shipment) — RecordsProcessed is their visible
/// proof-of-work in the meantime. Shopify is the first adapter to actually wire a
/// per-record domain event end to end, via <see cref="SyncedOrders"/> below.
/// </summary>
/// <param name="Succeeded">Whether the sync attempt completed without error.</param>
/// <param name="RecordsProcessed">Count of external records fetched and parsed.</param>
/// <param name="ErrorMessage">Null on success; a human-readable cause on failure. Never contains a secret value.</param>
public sealed record ConnectorSyncResult(bool Succeeded, int RecordsProcessed, string? ErrorMessage)
{
    /// <summary>
    /// Additive, optional: raw order data an adapter parsed this run, handed up to
    /// TriggerConnectorSyncCommandHandler to raise as ShopifyOrderSynced domain events
    /// through the already-tracked ConnectorSyncRun aggregate
    /// (<c>ConnectorSyncRun.RecordShopifyOrderSynced</c>) — this codebase's convention is
    /// that only aggregates raise domain events (<c>AggregateRoot.Raise</c> is only ever
    /// called from within an aggregate's own method), so an Infrastructure-layer adapter
    /// cannot raise the event itself. Adapters with nothing to report here (a failed
    /// sync, or a provider with no per-record events wired yet, e.g. Razorpay/Stripe/
    /// Shiprocket/Delhivery/Ondc) simply leave this at its default empty array — existing
    /// positional-constructor call sites (<c>new ConnectorSyncResult(true, 5, null)</c>)
    /// keep compiling unchanged.
    /// </summary>
    public IReadOnlyList<SyncedOrderRecord> SyncedOrders { get; init; } = Array.Empty<SyncedOrderRecord>();
}

/// <summary>Raw order data an adapter parsed from an external API/webhook, handed up to the command handler to raise as a ShopifyOrderSynced domain event.</summary>
public sealed record SyncedOrderRecord(string ExternalOrderId, string? CustomerEmail, decimal TotalAmount, DateTimeOffset OrderedAt);
