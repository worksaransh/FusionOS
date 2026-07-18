using FusionOS.SharedKernel;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections.Events;

namespace FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections;

/// <summary>
/// Phase 9 — Integration Hub, first slice: a company's configured
/// connection to an IntegrationConnector, Connected → Disconnected, or
/// flagged Error if a real sync attempt (once one exists) fails.
/// IntegrationConnectorId is a same-module FK, existence-validated in the
/// command handler (mirrors CreateBudgetLine/AccountId,
/// InstallPlugin/PluginListingId).
///
/// Deliberately does NOT store any credential/API-key/OAuth token — that is
/// a genuinely security-sensitive concern (encrypted-at-rest secrets
/// management, e.g. a vault integration) this slice does not attempt to
/// half-wire; <see cref="Label"/> is a plain human-readable name for the
/// connection ("Main Shopify Store"), not a place secrets live. There is
/// also no real sync engine — this is connection bookkeeping only, same
/// restraint as Marketplace's PluginInstallation not executing plugin code.
/// </summary>
public sealed class ConnectorConnection : TenantAggregateRoot
{
    public Guid IntegrationConnectorId { get; private set; }
    public string Label { get; private set; } = default!;
    public ConnectionStatus Status { get; private set; }
    public DateTimeOffset ConnectedAt { get; private set; }
    public DateTimeOffset? DisconnectedAt { get; private set; }

    private ConnectorConnection() { }

    public static ConnectorConnection Create(Guid companyId, Guid integrationConnectorId, string label)
    {
        if (integrationConnectorId == Guid.Empty)
            throw new ArgumentException("Integration connector id is required.", nameof(integrationConnectorId));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("A label for this connection is required.", nameof(label));

        var connection = new ConnectorConnection
        {
            CompanyId = companyId,
            IntegrationConnectorId = integrationConnectorId,
            Label = label.Trim(),
            Status = ConnectionStatus.Connected,
            ConnectedAt = DateTimeOffset.UtcNow,
        };

        connection.Raise(new ConnectorConnected(connection.Id, companyId, integrationConnectorId));
        return connection;
    }

    /// <summary>Reversible via a fresh <see cref="Create"/> representing reconnecting — same "terminal, re-add to redo" shape as PluginInstallation.Uninstall.</summary>
    public void Disconnect()
    {
        if (Status == ConnectionStatus.Disconnected)
            throw new InvalidOperationException("This connection is already disconnected.");

        Status = ConnectionStatus.Disconnected;
        DisconnectedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Flags a failed sync attempt — the future hook for a real sync engine to report back through, not exercised by any producer yet.</summary>
    public void MarkError()
    {
        if (Status == ConnectionStatus.Disconnected)
            throw new InvalidOperationException("A disconnected connection cannot be marked as errored — reconnect first.");

        Status = ConnectionStatus.Error;
    }
}
