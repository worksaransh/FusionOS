using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.MarkConnectorConnectionError;

/// <summary>
/// A manual flag for now — there is no automated sync/health-check engine yet to detect a
/// failure itself and call this (see ConnectorConnection's own class doc comment), same
/// "manual first, event-fed later" restraint as BusinessIntelligence's RecordKpiSnapshotCommand.
/// </summary>
public sealed record MarkConnectorConnectionErrorCommand(Guid CompanyId, Guid ConnectorConnectionId)
    : ICommand<ConnectorConnectionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.connection.mark-error" };
    public string EntityType => nameof(Domain.ConnectorConnections.ConnectorConnection);
    public Guid EntityId => ConnectorConnectionId;
    public string Action => "MarkedError";
}
