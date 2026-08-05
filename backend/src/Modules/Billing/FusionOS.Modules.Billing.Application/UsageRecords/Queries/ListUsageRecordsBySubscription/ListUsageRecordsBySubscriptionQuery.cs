using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.UsageRecords.Contracts;

namespace FusionOS.Modules.Billing.Application.UsageRecords.Queries.ListUsageRecordsBySubscription;

public sealed record ListUsageRecordsBySubscriptionQuery(Guid CompanyId, Guid SubscriptionId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<UsageRecordDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "billing.usage-record.read" };
}
