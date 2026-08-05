namespace FusionOS.Modules.Billing.Application.UsageRecords.Contracts;

public interface IUsageRecordRepository
{
    Task AddAsync(Domain.UsageRecords.UsageRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.UsageRecords.UsageRecord>> ListBySubscriptionAsync(Guid companyId, Guid subscriptionId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountBySubscriptionAsync(Guid companyId, Guid subscriptionId, CancellationToken cancellationToken = default);
}
