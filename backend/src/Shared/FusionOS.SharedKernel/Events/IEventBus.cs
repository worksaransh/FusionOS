namespace FusionOS.SharedKernel.Events;

/// <summary>Publishes integration events onto the durable event backbone (Kafka).</summary>
public interface IEventBus
{
    /// <summary>Publish a strongly-typed integration event (used by modules that construct one directly).</summary>
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish from the primitive fields stored on an outbox row, without needing to
    /// deserialize back into a concrete .NET type. This is what the generic
    /// OutboxDispatcher (FusionOS.BuildingBlocks.EventBus) uses so that ANY module's
    /// domain events are relayed to Kafka with zero per-module dispatch code
    /// (03_SYSTEM_ARCHITECTURE.md §4.2).
    /// </summary>
    Task PublishRawAsync(string eventType, Guid companyId, Guid eventId, DateTimeOffset occurredOn, string payloadJson, CancellationToken cancellationToken = default);
}
