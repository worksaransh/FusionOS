namespace FusionOS.BuildingBlocks.EventBus;

/// <summary>Bound from configuration section "Kafka" — 02_TECH_STACK.md §4.</summary>
public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TopicPrefix { get; set; } = "fusionos";
    public string ConsumerGroupId { get; set; } = "fusionos-host";
}
