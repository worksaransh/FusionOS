namespace FusionOS.SharedKernel.Events;

/// <summary>
/// Implemented by a module's own reaction to another module's integration event
/// (03_SYSTEM_ARCHITECTURE.md §4.2). Lives in SharedKernel (not
/// BuildingBlocks.EventBus) so Application-layer consumer classes can implement
/// it without an Application -> Infrastructure dependency. EventType must match
/// the producing module's OutboxMessage.EventType exactly (the raw domain event
/// class name, e.g. "GoodsReceiptLineReceived") — the consumer never references
/// the producing module's domain event type directly; it defines its own
/// payload shape matching the wire JSON, which is the point of decoupling
/// modules via events rather than shared types.
/// </summary>
public interface IIntegrationEventConsumer
{
    string EventType { get; }

    Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken);
}
