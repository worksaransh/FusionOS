using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConnectConnector;

public sealed record ConnectConnectorCommand(Guid CompanyId, Guid IntegrationConnectorId, string Label)
    : ICommand<ConnectorConnectionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.connection.connect" };
    public string EntityType => nameof(Domain.ConnectorConnections.ConnectorConnection);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
