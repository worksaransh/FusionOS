using FusionOS.SharedKernel;

namespace FusionOS.Modules.Billing.Domain.UsageRecords;

/// <summary>
/// Append-only metering record for a Subscription (e.g. MetricType="active-users",
/// MetricType="api-calls") — same append-only shape and lifecycle as
/// FusionOS.Modules.Finance.Domain.WithholdingTaxTransactions.WithholdingTaxTransaction: no
/// update/deactivate method exists here, because a usage record is a fact about what happened
/// at RecordedAt, not mutable state. MetricType is deliberately a free-form string rather than
/// an enum — new metrics (seats, storage, API calls, ...) are expected to be added over time
/// without a domain/migration change, the same "opaque discriminator string" tradeoff
/// WithholdingTaxTransaction.ReferenceType makes for the same reason.
/// </summary>
public sealed class UsageRecord : TenantAggregateRoot
{
    public Guid SubscriptionId { get; private set; }
    public string MetricType { get; private set; } = default!;
    public int Quantity { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }

    private UsageRecord() { }

    public static UsageRecord Create(Guid companyId, Guid subscriptionId, string metricType, int quantity)
    {
        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("Subscription id is required.", nameof(subscriptionId));
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type is required.", nameof(metricType));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        return new UsageRecord
        {
            CompanyId = companyId,
            SubscriptionId = subscriptionId,
            MetricType = metricType.Trim(),
            Quantity = quantity,
            RecordedAt = DateTimeOffset.UtcNow,
        };
    }
}
