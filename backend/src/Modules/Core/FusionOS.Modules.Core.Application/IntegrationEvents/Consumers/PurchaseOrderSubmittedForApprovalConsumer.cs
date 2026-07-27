using System.Text.Json;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Core.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to Procurement's PurchaseOrderSubmittedForApproval (relayed via the
/// outbox to Kafka exactly like every other cross-module event) by creating a
/// real ApprovalRequest in Core's generic, multi-step Approval workflow
/// engine — the event-driven wiring ApprovalRequest.cs's own doc comment
/// deferred as "a separate, later migration" (2026-07-22). Defines its own
/// local payload shape rather than referencing Procurement's domain event CLR
/// type, same reviewed pattern as every other cross-module consumer in this
/// codebase (e.g. Finance's PurchaseOrderApprovedConsumer) — this consumer
/// never takes a compile-time dependency on Procurement's internals, and
/// Procurement's Application/Domain projects have no reference back to Core
/// either, so nothing here creates the illegal cross-module coupling the
/// module boundary rule (03_SYSTEM_ARCHITECTURE.md §2) forbids.
///
/// <b>Approver selection:</b> this consumer has no visibility into
/// Procurement's org chart or any "who approves POs" policy — same stance as
/// ApprovalRequest.cs itself, which explicitly leaves that decision to the
/// caller. The pragmatic default used here: every user in the company who
/// holds the procurement.purchase-order.approve permission, excluding the
/// requester (maker-checker — also re-enforced by ApprovalRequest.Submit
/// itself, which throws if the requester appears in the approver list). If
/// that set is empty (e.g. a company with only one privileged user, who
/// happens to be the requester), there is nobody the workflow engine could
/// route this to yet — the event is still marked processed (so Kafka does
/// not retry it forever over a state that will not resolve itself), but no
/// ApprovalRequest is created. The PurchaseOrder stays PendingApproval; it
/// can still be approved directly via the pre-existing
/// ApprovePurchaseOrderCommand path (by a second privileged user, satisfying
/// its own maker-checker guard), or a future admin action could reassign
/// permissions and resubmit. This is a documented, deliberate limitation, not
/// a bug — see the class-level report on this feature for detail.
/// </summary>
public sealed class PurchaseOrderSubmittedForApprovalConsumer : IIntegrationEventConsumer
{
    private const string ApproverPermissionCode = "procurement.purchase-order.approve";

    private readonly IApprovalRequestRepository _approvalRequests;
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderSubmittedForApprovalConsumer(
        IApprovalRequestRepository approvalRequests,
        INotificationRepository notifications,
        IUserRepository users,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _approvalRequests = approvalRequests;
        _notifications = notifications;
        _users = users;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "PurchaseOrderSubmittedForApproval";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize PurchaseOrderSubmittedForApproval payload for event {eventId}.");

        if (payload.PurchaseOrderId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderSubmittedForApproval event {eventId} is missing PurchaseOrderId.");
        if (payload.CompanyId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderSubmittedForApproval event {eventId} is missing CompanyId.");
        if (payload.RequestedByUserId == Guid.Empty)
            throw new InvalidOperationException($"PurchaseOrderSubmittedForApproval event {eventId} is missing RequestedByUserId.");

        var approverUserIds = (await _users.ListUserIdsWithPermissionAsync(payload.CompanyId, ApproverPermissionCode, cancellationToken))
            .Where(userId => userId != payload.RequestedByUserId)
            .ToList();

        if (approverUserIds.Count > 0)
        {
            var approvalRequest = Domain.Workflow.ApprovalRequest.Submit(
                payload.CompanyId, "PurchaseOrder", payload.PurchaseOrderId, payload.RequestedByUserId, approverUserIds);
            await _approvalRequests.AddAsync(approvalRequest, cancellationToken);

            // Same-transaction, in-process notification — no outbox/consumer
            // round trip needed since Notification lives in this same Core
            // module/DbContext, same convention as CreateApprovalRequestCommandHandler.
            var notification = Notification.Create(
                payload.CompanyId,
                approverUserIds[0],
                "Approval requested",
                string.IsNullOrWhiteSpace(payload.Description)
                    ? $"You have a pending approval request for PurchaseOrder {payload.PurchaseOrderId}."
                    : payload.Description);
            await _notifications.AddAsync(notification, cancellationToken);
        }

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the ApprovalRequest/Notification writes and
        // the idempotency marker share the same scoped DbContext instance, so
        // this commits both atomically (or neither, if something throws first).
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(Guid PurchaseOrderId, Guid CompanyId, Guid RequestedByUserId, decimal Amount, string? Description);
}
