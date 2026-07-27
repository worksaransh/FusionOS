using FusionOS.SharedKernel;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorSyncRuns.Events;

namespace FusionOS.Modules.IntegrationHub.Domain.ConnectorSyncRuns;

/// <summary>
/// Phase 8 — Integration Hub sync run history: one row per attempt to pull/push data
/// through a ConnectorConnection, whether triggered manually
/// (TriggerConnectorSyncCommand, dispatching to whichever IConnectorSyncAdapter's
/// ProviderName matches the connection's IntegrationConnector.Provider) or by an
/// inbound webhook delivery (WebhooksController). ConnectorConnectionId is a
/// same-module FK, existence-validated in the command handler — same convention as
/// ConnectorConnection.IntegrationConnectorId (mirrors CreateBudgetLine/AccountId,
/// InstallPlugin/PluginListingId).
///
/// This aggregate also doubles as the one through which provider-specific "something
/// happened" events (e.g. <see cref="Events.ShopifyOrderSynced"/>) are raised. This
/// codebase's established convention is that only aggregates raise domain events —
/// <c>AggregateRoot.Raise</c> is <c>protected</c>, callable only from within an
/// aggregate's own method, and dispatched to the outbox via BaseDbContext walking the
/// EF change tracker. Inventing a separate "marker aggregate" purely to raise events
/// would duplicate what a sync run already represents (one attempt to bring in
/// external data); a run created by TriggerConnectorSyncCommandHandler or by
/// WebhooksController (for the webhook path) is already tracked by the same DbContext
/// instance for the rest of that request, so calling
/// <see cref="RecordShopifyOrderSynced"/> on it before <c>SaveChangesAsync</c> stages
/// the event onto this module's own outbox exactly like ConnectorConnected does today.
/// </summary>
public sealed class ConnectorSyncRun : TenantAggregateRoot
{
    public Guid ConnectorConnectionId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public SyncRunStatus Status { get; private set; }
    public int RecordsProcessed { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ConnectorSyncRun() { }

    public static ConnectorSyncRun Start(Guid companyId, Guid connectorConnectionId)
    {
        if (connectorConnectionId == Guid.Empty)
            throw new ArgumentException("Connector connection id is required.", nameof(connectorConnectionId));

        return new ConnectorSyncRun
        {
            CompanyId = companyId,
            ConnectorConnectionId = connectorConnectionId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = SyncRunStatus.Running,
            RecordsProcessed = 0,
        };
    }

    public void Complete(int recordsProcessed)
    {
        if (Status != SyncRunStatus.Running)
            throw new InvalidOperationException("Only a running sync run can be completed.");
        if (recordsProcessed < 0)
            throw new ArgumentOutOfRangeException(nameof(recordsProcessed), "Records processed cannot be negative.");

        Status = SyncRunStatus.Succeeded;
        RecordsProcessed = recordsProcessed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        if (Status != SyncRunStatus.Running)
            throw new InvalidOperationException("Only a running sync run can be failed.");
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("An error message is required.", nameof(errorMessage));

        Status = SyncRunStatus.Failed;
        ErrorMessage = errorMessage.Trim();
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Raises a <see cref="ShopifyOrderSynced"/> domain event for one order this run
    /// pulled from Shopify's REST Admin API or received via the Shopify webhook — the
    /// establishing convention for producing a fresh domain event from
    /// Application/Infrastructure-layer code that isn't reacting to another module's
    /// event, just an aggregate method raising its own event like any other.
    /// </summary>
    public void RecordShopifyOrderSynced(string shopifyOrderId, string customerEmail, decimal totalAmount, DateTimeOffset orderedAt)
    {
        if (string.IsNullOrWhiteSpace(shopifyOrderId))
            throw new ArgumentException("Shopify order id is required.", nameof(shopifyOrderId));

        Raise(new ShopifyOrderSynced(CompanyId, shopifyOrderId, customerEmail ?? string.Empty, totalAmount, orderedAt));
    }
}
