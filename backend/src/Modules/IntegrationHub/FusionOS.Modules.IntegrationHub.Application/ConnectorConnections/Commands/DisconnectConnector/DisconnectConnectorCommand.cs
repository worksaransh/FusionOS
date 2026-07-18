using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.DisconnectConnector;

public sealed record DisconnectConnectorCommand(Guid CompanyId, Guid ConnectorConnectionId)
    : ICommand<ConnectorConnectionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.connection.disconnect" };
    public string EntityType => nameof(Domain.ConnectorConnections.ConnectorConnection);
    public Guid EntityId => ConnectorConnectionId;
    public string Action => "Disconnected";
}
