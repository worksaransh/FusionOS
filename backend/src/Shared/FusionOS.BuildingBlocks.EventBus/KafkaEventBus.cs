using System.Text.Json;
using Confluent.Kafka;
using FusionOS.SharedKernel.Events;
using Microsoft.Extensions.Options;

namespace FusionOS.BuildingBlocks.EventBus;

/// <summary>
/// Publishes integration events to Kafka wrapped in a CloudEvents-shaped envelope
/// (03_SYSTEM_ARCHITECTURE.md §4.2). Topic is namespaced per event type so
/// consumers can subscribe narrowly: "{prefix}.{eventType-lowercased}".
/// </summary>
public sealed class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;

    public KafkaEventBus(IOptions<KafkaOptions> options)
    {
        _options = options.Value;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All,
        }).Build();
    }

    public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var envelope = new
        {
            id = integrationEvent.Id,
            source = integrationEvent.Source,
            type = integrationEvent.EventType,
            time = integrationEvent.OccurredOn,
            datacontenttype = "application/json",
            data = integrationEvent,
        };

        await ProduceAsync(integrationEvent.EventType, integrationEvent.CompanyId, JsonSerializer.Serialize(envelope), cancellationToken);
    }

    public async Task PublishRawAsync(string eventType, Guid companyId, Guid eventId, DateTimeOffset occurredOn, string payloadJson, CancellationToken cancellationToken = default)
    {
        using var payloadDocument = JsonDocument.Parse(payloadJson);
        var envelope = new
        {
            id = eventId,
            source = "fusionos",
            type = eventType,
            time = occurredOn,
            datacontenttype = "application/json",
            data = payloadDocument.RootElement,
        };

        await ProduceAsync(eventType, companyId, JsonSerializer.Serialize(envelope), cancellationToken);
    }

    private async Task ProduceAsync(string eventType, Guid companyId, string envelopeJson, CancellationToken cancellationToken)
    {
        var topic = $"{_options.TopicPrefix}.{eventType.ToLowerInvariant()}";
        var message = new Message<string, string>
        {
            Key = companyId.ToString(),
            Value = envelopeJson,
        };

        await _producer.ProduceAsync(topic, message, cancellationToken);
    }

    public void Dispose() => _producer.Dispose();
}
