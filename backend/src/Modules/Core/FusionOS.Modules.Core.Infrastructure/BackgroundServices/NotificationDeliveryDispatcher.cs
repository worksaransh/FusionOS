using FusionOS.Modules.Core.Application.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FusionOS.Modules.Core.Infrastructure.BackgroundServices;

/// <summary>
/// Polls for undelivered notifications and hands them to
/// NotificationDeliveryService — the thin BackgroundService wrapper around the
/// actual (testable) delivery logic, structurally mirroring
/// FusionOS.BuildingBlocks.EventBus.OutboxDispatcher&lt;TContext&gt; (same
/// scope-per-poll, try/catch-and-log-per-cycle, fixed interval shape). A
/// longer interval than the outbox's 5s — email delivery is not on the
/// transactional-consistency critical path the outbox serves, and a slower
/// poll is gentler on SendGrid's own rate limits.
/// </summary>
public sealed class NotificationDeliveryDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationDeliveryDispatcher> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);
    private const int BatchSize = 50;

    public NotificationDeliveryDispatcher(IServiceScopeFactory scopeFactory, ILogger<NotificationDeliveryDispatcher> logger)
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
                using var scope = _scopeFactory.CreateScope();
                var deliveryService = scope.ServiceProvider.GetRequiredService<NotificationDeliveryService>();
                var processed = await deliveryService.DeliverPendingAsync(BatchSize, stoppingToken);
                if (processed > 0)
                    _logger.LogInformation("Notification delivery dispatcher processed {Count} pending notifications", processed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification delivery dispatch cycle failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}
