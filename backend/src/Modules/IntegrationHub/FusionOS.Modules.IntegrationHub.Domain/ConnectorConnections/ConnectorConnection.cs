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
/// connection ("Main Shopify Store"), not a place secrets live.
///
/// <b>Phase 8 — credential reference (2026-07-25):</b> <see cref="CredentialReference"/>
/// is the one addition that touches secrets at all, and it deliberately never stores
/// a secret value — only a STRING NAME for where the real secret lives externally
/// (an environment variable / configuration key, e.g. "SHOPIFY_STORE_ACME_API_KEY", or
/// a secrets-manager key path). The actual secret is read at call time from
/// <c>IConfiguration</c> by whichever <c>IConnectorSyncAdapter</c> implementation
/// needs it (Application/Infrastructure layer, not referenced from here) — this
/// application still never persists a real credential value, honoring the restraint
/// this class's doc comment always called for. There is also still no real sync
/// engine wired into THIS aggregate — see ConnectorSyncRun for that (a sibling
/// aggregate, not a change to this one), same restraint as Marketplace's
/// PluginInstallation not executing plugin code.
/// </summary>
public sealed class ConnectorConnection : TenantAggregateRoot
{
    public Guid IntegrationConnectorId { get; private set; }
    public string Label { get; private set; } = default!;
    public ConnectionStatus Status { get; private set; }
    public DateTimeOffset ConnectedAt { get; private set; }
    public DateTimeOffset? DisconnectedAt { get; private set; }

    /// <summary>Name of the external config/environment key or secrets-manager path where the real credential lives — never the secret value itself. Null until <see cref="ConfigureCredential"/> is called.</summary>
    public string? CredentialReference { get; private set; }

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

    /// <summary>
    /// Sets (or replaces) the credential reference — settable any time while the
    /// connection is Connected, so a rotated secret (new env var name) can be pointed
    /// to without disconnecting/reconnecting. Never a secret value: validated here only
    /// as a non-empty reference string, not as looking like a real key/token, since this
    /// aggregate has no business knowing what a given provider's real credentials look like.
    /// </summary>
    public void ConfigureCredential(string credentialReference)
    {
        if (string.IsNullOrWhiteSpace(credentialReference))
            throw new ArgumentException("A credential reference is required.", nameof(credentialReference));
        if (Status != ConnectionStatus.Connected)
            throw new InvalidOperationException("A credential reference can only be configured while the connection is Connected.");

        CredentialReference = credentialReference.Trim();
    }
}
