namespace FusionOS.Modules.Billing.Application.UsageRecords.Contracts;

public sealed record UsageRecordDto(Guid Id, Guid SubscriptionId, string MetricType, int Quantity, DateTimeOffset RecordedAt);
