using System.Text.Json;
using Confluent.Kafka;
using FusionOS.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FusionOS.BuildingBlocks.EventBus;

/// <summary>
/// The consumer side of 03_SYSTEM_ARCHITECTURE.md §4.2's cross-module event
/// architecture — the piece that was missing after several rounds of building
/// producers. Discovers every registered <see cref="IIntegrationEventConsumer"/>
/// across every module (regardless of which module registered it — this class
/// has no per-module knowledge), subscribes to their topics, and dispatches each
/// message to the matching consumer(s) by EventType. Offsets are committed only
/// after a successful dispatch, giving at-least-once delivery — consumers are
/// expected to be idempotent via the ProcessedIntegrationEvent ledger
/// (BuildingBlocks.Infrastructure), not this class's job to deduplicate.
/// </summary>
public sealed class KafkaConsumerHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaConsumerHostedService> _logger;

    public KafkaConsumerHostedService(IServiceScopeFactory scopeFactory, IOptions<KafkaOptions> options, ILogger<KafkaConsumerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<string> eventTypes;
        using (var startupScope = _scopeFactory.CreateScope())
        {
            eventTypes = startupScope.ServiceProvider.GetServices<IIntegrationEventConsumer>()
                .Select(c => c.EventType)
                .Distinct()
                .ToList();
        }

        if (eventTypes.Count == 0)
        {
            _logger.LogInformation("No IIntegrationEventConsumer registered by any module — Kafka consumer host has nothing to subscribe to.");
            return;
        }

        var topics = eventTypes.Select(t => $"{_options.TopicPrefix}.{t.ToLowerInvariant()}").ToList();

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        }).Build();

        consumer.Subscribe(topics);
        _logger.LogInformation("Kafka consumer host subscribed to {TopicCount} topic(s): {Topics}", topics.Count, string.Join(", ", topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? result;
            try
            {
                result = consumer.Consume(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consume loop error");
                continue;
            }

            if (result?.Message is null)
                continue;

            try
            {
                await DispatchAsync(result.Message.Value, stoppingToken);
                consumer.Commit(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch integration event message from {Topic}; offset not committed, will retry on redelivery", result.Topic);
            }
        }
    }

    private async Task DispatchAsync(string envelopeJson, CancellationToken cancellationToken)
    {
        using var envelope = JsonDocument.Parse(envelopeJson);
        var root = envelope.RootElement;
        var eventType = root.GetProperty("type").GetString()!;
        var eventId = root.GetProperty("id").GetGuid();
        var data = root.GetProperty("data");
        var companyId = data.TryGetProperty("CompanyId", out var companyIdProp) ? companyIdProp.GetGuid() : Guid.Empty;
        var payloadJson = data.GetRawText();

        using var scope = _scopeFactory.CreateScope();
        var matchingConsumers = scope.ServiceProvider.GetServices<IIntegrationEventConsumer>()
            .Where(c => c.EventType == eventType)
            .ToList();

        foreach (var consumer in matchingConsumers)
        {
            await consumer.HandleAsync(eventId, companyId, payloadJson, cancellationToken);
        }
    }
}
