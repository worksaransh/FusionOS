using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Application.Sync;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Commands.TriggerConnectorSync;

/// <summary>
/// Looks up the connection's IntegrationConnector.Provider string, picks the
/// IConnectorSyncAdapter whose ProviderName matches it (case-insensitive) out of
/// every adapter registered in DI (IEnumerable&lt;IConnectorSyncAdapter&gt; — see
/// IntegrationHubModule.cs), invokes it, and records the outcome as a
/// ConnectorSyncRun. A run row is created even when no adapter is registered for the
/// provider, so "why did my sync never run" has a visible, queryable answer
/// (ListConnectorSyncRunsQuery) rather than a silent no-op.
///
/// Per-record domain events (currently only Shopify's ShopifyOrderSynced) are raised
/// here, not inside the adapter — this codebase's convention is that only aggregates
/// raise domain events, and the freshly-created <c>run</c> is already tracked by this
/// handler's DbContext, so calling <c>run.RecordShopifyOrderSynced(...)</c> before
/// SaveChangesAsync stages each event onto the outbox exactly like any other domain
/// event raised inside an aggregate method.
/// </summary>
public sealed class TriggerConnectorSyncCommandHandler : IRequestHandler<TriggerConnectorSyncCommand, ConnectorSyncRunDto>
{
    private readonly IConnectorConnectionRepository _connectionRepository;
    private readonly IIntegrationConnectorRepository _integrationConnectorRepository;
    private readonly IConnectorSyncRunRepository _syncRunRepository;
    private readonly IEnumerable<IConnectorSyncAdapter> _adapters;
    private readonly IUnitOfWork _unitOfWork;

    public TriggerConnectorSyncCommandHandler(
        IConnectorConnectionRepository connectionRepository,
        IIntegrationConnectorRepository integrationConnectorRepository,
        IConnectorSyncRunRepository syncRunRepository,
        IEnumerable<IConnectorSyncAdapter> adapters,
        IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _integrationConnectorRepository = integrationConnectorRepository;
        _syncRunRepository = syncRunRepository;
        _adapters = adapters;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConnectorSyncRunDto> Handle(TriggerConnectorSyncCommand request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.CompanyId, request.ConnectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Connector connection '{request.ConnectionId}' was not found.");

        var connector = await _integrationConnectorRepository.GetByIdAsync(request.CompanyId, connection.IntegrationConnectorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Integration connector '{connection.IntegrationConnectorId}' was not found.");

        var run = Domain.ConnectorSyncRuns.ConnectorSyncRun.Start(request.CompanyId, connection.Id);
        await _syncRunRepository.AddAsync(run, cancellationToken);

        var adapter = _adapters.FirstOrDefault(a => string.Equals(a.ProviderName, connector.Provider, StringComparison.OrdinalIgnoreCase));
        if (adapter is null)
        {
            run.Fail($"No sync adapter is registered for provider '{connector.Provider}'.");
        }
        else
        {
            var result = await adapter.SyncAsync(request.CompanyId, connection.CredentialReference, cancellationToken);
            if (result.Succeeded)
            {
                run.Complete(result.RecordsProcessed);
                foreach (var order in result.SyncedOrders)
                {
                    run.RecordShopifyOrderSynced(order.ExternalOrderId, order.CustomerEmail ?? string.Empty, order.TotalAmount, order.OrderedAt);
                }
            }
            else
            {
                run.Fail(result.ErrorMessage ?? "The sync adapter reported failure without an error message.");
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ConnectorSyncRunMapper.ToDto(run);
    }
}
