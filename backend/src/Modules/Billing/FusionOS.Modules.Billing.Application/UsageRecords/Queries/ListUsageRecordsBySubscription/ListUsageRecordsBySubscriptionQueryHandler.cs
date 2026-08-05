using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.UsageRecords.Contracts;
using MediatR;

namespace FusionOS.Modules.Billing.Application.UsageRecords.Queries.ListUsageRecordsBySubscription;

public sealed class ListUsageRecordsBySubscriptionQueryHandler : IRequestHandler<ListUsageRecordsBySubscriptionQuery, PagedResult<UsageRecordDto>>
{
    private readonly IUsageRecordRepository _repository;

    public ListUsageRecordsBySubscriptionQueryHandler(IUsageRecordRepository repository) => _repository = repository;

    public async Task<PagedResult<UsageRecordDto>> Handle(ListUsageRecordsBySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.ListBySubscriptionAsync(request.CompanyId, request.SubscriptionId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountBySubscriptionAsync(request.CompanyId, request.SubscriptionId, cancellationToken);

        return new PagedResult<UsageRecordDto>(
            records.Select(r => new UsageRecordDto(r.Id, r.SubscriptionId, r.MetricType, r.Quantity, r.RecordedAt)).ToList(),
            request.Page,
            request.PageSize,
            total);
    }
}
