using FusionOS.SharedKernel.Events;
using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FusionOS.BuildingBlocks.EventBus;

/// <summary>
/// Polls unprocessed outbox rows across every module's DbContext and relays them
/// to Kafka — the "Outbox Relay" described in 03_SYSTEM_ARCHITECTURE.md §4.2.
/// One instance is registered per module DbContext type at the Host composition root.
/// Generic by design: because OutboxMessage carries EventType, CompanyId, and the
/// raw JSON payload, this dispatcher needs no per-module mapping code to publish —
/// any module's domain events are relayed automatically once staged by BaseDbContext.
/// </summary>
public sealed class OutboxDispatcher<TContext> : BackgroundService where TContext : BaseDbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcher<TContext>> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    public OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher<TContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatch failed for {ContextType}", typeof(TContext).Name);
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task DispatchPendingAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var pending = await context.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(50)
            .ToListAsync(stoppingToken);

        var anyChanges = false;

        foreach (var message in pending)
        {
            try
            {
                await eventBus.PublishRawAsync(message.EventType, message.CompanyId, message.Id, message.OccurredOn, message.Content, stoppingToken);
                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                // At-least-once delivery per 03_SYSTEM_ARCHITECTURE.md §4.2 point 4 — leave
                // ProcessedOn null so the next poll retries; record the error for visibility.
                _logger.LogWarning(ex, "Failed to publish outbox message {MessageId} ({EventType})", message.Id, message.EventType);
                message.MarkFailed(ex.Message);
            }

            anyChanges = true;
        }

        if (anyChanges)
            await context.SaveChangesAsync(stoppingToken);
    }
}
