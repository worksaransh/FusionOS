using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Commands.TriggerConnectorSync;

public sealed record TriggerConnectorSyncCommand(Guid CompanyId, Guid ConnectionId)
    : ICommand<ConnectorSyncRunDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.sync.trigger" };
    public string EntityType => nameof(Domain.ConnectorSyncRuns.ConnectorSyncRun);
    public Guid EntityId { get; init; }
    public string Action => "Triggered";
}
