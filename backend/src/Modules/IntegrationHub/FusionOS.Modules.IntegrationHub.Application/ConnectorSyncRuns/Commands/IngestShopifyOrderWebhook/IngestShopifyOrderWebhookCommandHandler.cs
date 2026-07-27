using System.Globalization;
using System.Text.Json;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorSyncRuns;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Commands.IngestShopifyOrderWebhook;

/// <summary>
/// Parses a single Shopify order payload (an "orders/create"/"orders/updated" webhook
/// delivers ONE order object directly — unlike ShopifyConnectorSyncAdapter's
/// <c>orders.json</c> list response, there is no wrapping <c>{"orders": [...]}</c>
/// envelope here) and records it the same way the polling adapter does: a
/// ConnectorSyncRun row, with a ShopifyOrderSynced domain event raised through it on
/// success — one aggregate, one place events get raised from, whether the sync was
/// triggered by TriggerConnectorSyncCommand or by this webhook.
/// </summary>
public sealed class IngestShopifyOrderWebhookCommandHandler : IRequestHandler<IngestShopifyOrderWebhookCommand, Unit>
{
    private readonly IConnectorSyncRunRepository _syncRunRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IngestShopifyOrderWebhookCommandHandler(IConnectorSyncRunRepository syncRunRepository, IUnitOfWork unitOfWork)
    {
        _syncRunRepository = syncRunRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(IngestShopifyOrderWebhookCommand request, CancellationToken cancellationToken)
    {
        var run = ConnectorSyncRun.Start(request.CompanyId, request.ConnectionId);
        await _syncRunRepository.AddAsync(run, cancellationToken);

        try
        {
            using var document = JsonDocument.Parse(request.RawBody);
            var order = document.RootElement;

            var id = order.TryGetProperty("id", out var idProp) ? idProp.GetRawText() : null;
            var email = order.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String
                ? emailProp.GetString()
                : null;
            var totalPriceRaw = order.TryGetProperty("total_price", out var totalProp) ? totalProp.GetString() : null;
            var createdAtRaw = order.TryGetProperty("created_at", out var createdProp) ? createdProp.GetString() : null;

            if (string.IsNullOrWhiteSpace(id))
            {
                run.Fail("Shopify webhook payload did not contain an order id.");
            }
            else
            {
                var totalAmount = decimal.TryParse(totalPriceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedTotal)
                    ? parsedTotal
                    : 0m;
                var orderedAt = DateTimeOffset.TryParse(createdAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                    ? parsedDate
                    : DateTimeOffset.UtcNow;

                run.RecordShopifyOrderSynced(id, email ?? string.Empty, totalAmount, orderedAt);
                run.Complete(1);
            }
        }
        catch (JsonException ex)
        {
            run.Fail($"Could not parse Shopify webhook payload: {ex.Message}");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
