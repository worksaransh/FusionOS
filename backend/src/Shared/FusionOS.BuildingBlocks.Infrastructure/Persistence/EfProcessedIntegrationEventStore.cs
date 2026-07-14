using FusionOS.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Generic implementation of <see cref="IProcessedIntegrationEventStore"/>, one
/// instance per module DbContext type — the same "no per-module plumbing" pattern
/// as OutboxDispatcher&lt;TContext&gt; (FusionOS.BuildingBlocks.EventBus). Each module
/// registers this once in its own RegisterServices, e.g.:
/// services.AddScoped&lt;IProcessedIntegrationEventStore, EfProcessedIntegrationEventStore&lt;InventoryDbContext&gt;&gt;();
/// </summary>
public sealed class EfProcessedIntegrationEventStore<TContext> : IProcessedIntegrationEventStore where TContext : BaseDbContext
{
    private readonly TContext _context;

    public EfProcessedIntegrationEventStore(TContext context) => _context = context;

    public Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        _context.ProcessedIntegrationEvents.AnyAsync(x => x.EventId == eventId, cancellationToken);

    public void MarkProcessed(Guid eventId, string eventType) =>
        _context.ProcessedIntegrationEvents.Add(ProcessedIntegrationEvent.Create(eventId, eventType));
}
