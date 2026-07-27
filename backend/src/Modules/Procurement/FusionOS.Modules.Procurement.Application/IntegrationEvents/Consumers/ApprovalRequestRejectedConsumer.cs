using System.Text.Json;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Procurement.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Core's ApprovalRequestRejected (raised by
/// FusionOS.Modules.Core.Domain.Workflow.ApprovalRequest.Decide) by rejecting
/// the matching PurchaseOrder — the rejection leg of the same round trip as
/// ApprovalRequestApprovedConsumer (2026-07-22). Same local-payload-shape and
/// EntityType-filter conventions as that consumer; see its doc comment.
/// </summary>
public sealed class ApprovalRequestRejectedConsumer : IIntegrationEventConsumer
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovalRequestRejectedConsumer(
        IPurchaseOrderRepository purchaseOrderRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "ApprovalRequestRejected";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize ApprovalRequestRejected payload for event {eventId}.");

        if (string.Equals(payload.EntityType, "PurchaseOrder", StringComparison.OrdinalIgnoreCase) && payload.EntityId != Guid.Empty)
        {
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(payload.CompanyId, payload.EntityId, cancellationToken);

            if (purchaseOrder is not null && purchaseOrder.Status == Domain.PurchaseOrders.PurchaseOrderStatus.PendingApproval)
            {
                purchaseOrder.Reject();
            }
        }

        _processedEvents.MarkProcessed(eventId, EventType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(Guid ApprovalRequestId, Guid CompanyId, string EntityType, Guid EntityId, Guid RequestedBy, int RejectedAtStepNumber);
}
