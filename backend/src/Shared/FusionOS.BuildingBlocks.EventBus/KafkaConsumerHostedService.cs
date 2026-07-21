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
///
/// <b>Poison-message handling (2026-07-21):</b> Confluent.Kafka/librdkafka does
/// not track a per-message redelivery count the way some other broker clients
/// do (there is no "delivery count" header to read off <see cref="ConsumeResult{TKey,TValue}"/>),
/// so this class keeps its own in-process counter in <see cref="_failuresByPartition"/>,
/// keyed by partition and keyed further by the specific offset that keeps
/// failing. A message that fails to dispatch <see cref="MaxDispatchAttempts"/>
/// times in a row (whether those attempts happen back-to-back or are spread
/// across a consumer-group rebalance that re-delivers the same un-committed
/// offset to this process) is logged at Critical and its offset is committed
/// anyway, so one malformed/poison message cannot wedge every message behind
/// it on the same partition forever. <b>This is a "don't permanently stall the
/// partition" mitigation, not a dead-letter queue</b> — the message is dropped,
/// not preserved anywhere for inspection or replay. A real DLQ (a dedicated
/// "{topic}.dlq" topic the poison message gets re-produced to, plus tooling to
/// inspect/replay it) is a larger follow-up this class deliberately does not
/// attempt.
/// </summary>
public sealed class KafkaConsumerHostedService : BackgroundService
{
    /// <summary>
    /// How many consecutive failed dispatch attempts for the exact same
    /// (partition, offset) this process tolerates before giving up on that
    /// message and committing past it. See the class doc comment's
    /// "Poison-message handling" note for why this is a bounded in-process
    /// counter rather than a true dead-letter queue.
    /// </summary>
    private const int MaxDispatchAttempts = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaConsumerHostedService> _logger;

    /// <summary>
    /// Tracks, per partition, the offset that most recently failed to dispatch
    /// and how many consecutive times it has failed. Only ever read/written
    /// from the single-threaded consume loop in <see cref="ExecuteAsync"/>, so
    /// no locking is needed.
    /// </summary>
    private readonly Dictionary<TopicPartition, (long Offset, int Attempts)> _failuresByPartition = new();

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
                _failuresByPartition.Remove(result.TopicPartition);
            }
            catch (Exception ex)
            {
                var attempts = RecordFailure(result.TopicPartitionOffset);
                if (attempts >= MaxDispatchAttempts)
                {
                    _logger.LogCritical(
                        ex,
                        "Giving up on integration event message at {Topic}/{Partition}@{Offset} after {Attempts} failed dispatch attempts; committing the offset anyway so the partition is not permanently stalled behind this message. " +
                        "This message is now DROPPED, not dead-lettered — there is no DLQ topic to inspect/replay it from yet (see KafkaConsumerHostedService's class doc comment).",
                        result.Topic, result.Partition.Value, result.Offset.Value, attempts);
                    consumer.Commit(result);
                    _failuresByPartition.Remove(result.TopicPartition);
                }
                else
                {
                    _logger.LogError(
                        ex,
                        "Failed to dispatch integration event message from {Topic}/{Partition}@{Offset} (attempt {Attempts}/{MaxAttempts}); offset not committed, will retry on redelivery",
                        result.Topic, result.Partition.Value, result.Offset.Value, attempts, MaxDispatchAttempts);
                }
            }
        }
    }

    /// <summary>
    /// Increments (or starts) the consecutive-failure count for the given
    /// (partition, offset), resetting it if this offset differs from whatever
    /// last failed on that partition — a new message means a clean slate.
    /// Returns the updated attempt count.
    /// </summary>
    private int RecordFailure(TopicPartitionOffset topicPartitionOffset)
    {
        var partition = topicPartitionOffset.TopicPartition;
        var offset = topicPartitionOffset.Offset.Value;

        var attempts = _failuresByPartition.TryGetValue(partition, out var existing) && existing.Offset == offset
            ? existing.Attempts + 1
            : 1;

        _failuresByPartition[partition] = (offset, attempts);
        return attempts;
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
