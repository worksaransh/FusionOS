namespace FusionOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Transactional outbox row — 03_SYSTEM_ARCHITECTURE.md §4.2. Written in the same
/// DB transaction as the business change; relayed to Kafka by the OutboxDispatcher
/// background service in FusionOS.BuildingBlocks.EventBus. CompanyId is carried
/// alongside the raw event JSON so the generic dispatcher can publish to Kafka
/// (topic key + CloudEvents envelope) without needing to deserialize back into a
/// concrete, module-specific event type.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = default!;
    public Guid CompanyId { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTimeOffset OccurredOn { get; private set; }
    public DateTimeOffset? ProcessedOn { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string eventType, Guid companyId, string jsonContent) => new()
    {
        Id = Guid.NewGuid(),
        EventType = eventType,
        CompanyId = companyId,
        Content = jsonContent,
        OccurredOn = DateTimeOffset.UtcNow,
    };

    public void MarkProcessed() => ProcessedOn = DateTimeOffset.UtcNow;
    public void MarkFailed(string error) => Error = error;
}
