namespace FusionOS.SharedKernel.Events;

/// <summary>
/// Idempotency check for cross-module integration event consumers
/// (03_SYSTEM_ARCHITECTURE.md §4.2 — "idempotent by dedupe on event id"). Backed
/// by the ProcessedIntegrationEvents table every module's DbContext gets
/// automatically via BaseDbContext (BuildingBlocks.Infrastructure). Application-layer
/// consumers depend on this interface rather than the DbContext directly, so they
/// stay clean of any Infrastructure-layer reference. MarkProcessed only stages the
/// record in the current change tracker — it does not save — so a consumer can call
/// it alongside its own writes and commit both atomically with one
/// IUnitOfWork.SaveChangesAsync() call, using the same scoped DbContext instance.
/// </summary>
public interface IProcessedIntegrationEventStore
{
    Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

    void MarkProcessed(Guid eventId, string eventType);
}
