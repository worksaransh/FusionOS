using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConfigureConnectorCredential;

/// <summary>
/// Sets the connection's <c>CredentialReference</c> — a STRING that names WHERE the
/// real secret lives externally (an environment variable name, e.g.
/// "SHOPIFY_STORE_ACME_API_KEY", or a secrets-manager key path), never the secret value
/// itself. This application never persists a real credential — see
/// ConnectorConnection's own class doc comment.
/// </summary>
public sealed record ConfigureConnectorCredentialCommand(Guid CompanyId, Guid ConnectionId, string CredentialReference)
    : ICommand<ConnectorConnectionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.credential.configure" };
    public string EntityType => nameof(Domain.ConnectorConnections.ConnectorConnection);
    public Guid EntityId => ConnectionId;
    public string Action => "CredentialConfigured";
}
