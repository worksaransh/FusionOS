using System.Text.Json;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Procurement.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Core's ApprovalRequestApproved (raised by
/// FusionOS.Modules.Core.Domain.Workflow.ApprovalRequest.Decide, relayed via
/// the outbox to Kafka exactly like every other cross-module event) by
/// approving the matching PurchaseOrder — closing the loop for
/// SubmitPurchaseOrderForApprovalCommand (2026-07-22). Defines its own local
/// payload shape rather than referencing Core's domain event CLR type, same
/// reviewed pattern as GoodsReceiptLineReceivedConsumer in this same module:
/// a consumer must never take a compile-time dependency on the producing
/// module's internals.
///
/// ApprovalRequest is generic (EntityType is caller-defined free text) and
/// could in principle carry approvals for entities this module doesn't own
/// at all, so EntityType != "PurchaseOrder" is a legitimate, silent no-op
/// here rather than an error.
/// </summary>
public sealed class ApprovalRequestApprovedConsumer : IIntegrationEventConsumer
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovalRequestApprovedConsumer(
        IPurchaseOrderRepository purchaseOrderRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "ApprovalRequestApproved";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize ApprovalRequestApproved payload for event {eventId}.");

        if (string.Equals(payload.EntityType, "PurchaseOrder", StringComparison.OrdinalIgnoreCase) && payload.EntityId != Guid.Empty)
        {
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(payload.CompanyId, payload.EntityId, cancellationToken);

            // A PO that's no longer PendingApproval (already approved/rejected
            // directly through some other path, or not found at all) is a
            // legitimate no-op — a Kafka consumer must never fail a whole
            // redelivery over a state it can't fully control, same convention
            // as GoodsReceiptLineReceivedConsumer's unmatched-product no-op.
            if (purchaseOrder is not null && purchaseOrder.Status == Domain.PurchaseOrders.PurchaseOrderStatus.PendingApproval)
            {
                purchaseOrder.Approve();
            }
        }

        _processedEvents.MarkProcessed(eventId, EventType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(Guid ApprovalRequestId, Guid CompanyId, string EntityType, Guid EntityId, Guid RequestedBy);
}
