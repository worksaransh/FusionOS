namespace FusionOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Idempotency ledger for consumed integration events — 03_SYSTEM_ARCHITECTURE.md
/// §4.2 point 3: "consumers, idempotent by design (dedupe on event id)". Every
/// module's DbContext gets this table automatically (same pattern as
/// OutboxMessage), so a consumer can check-and-record "already handled" inside
/// the SAME SaveChanges transaction as the side effect it performs — e.g.
/// Inventory's GoodsReceipt consumer inserts both the InventoryLedgerEntry and
/// this marker in one atomic write.
/// </summary>
public sealed class ProcessedIntegrationEvent
{
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = default!;
    public DateTimeOffset ProcessedAt { get; private set; }

    private ProcessedIntegrationEvent() { }

    public static ProcessedIntegrationEvent Create(Guid eventId, string eventType) => new()
    {
        EventId = eventId,
        EventType = eventType,
        ProcessedAt = DateTimeOffset.UtcNow,
    };
}
