using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using FusionOS.Modules.Billing.Application.UsageRecords.Contracts;
using FusionOS.Modules.Billing.Domain.UsageRecords;
using MediatR;

namespace FusionOS.Modules.Billing.Application.UsageRecords.Commands.RecordUsage;

public sealed class RecordUsageCommandHandler : IRequestHandler<RecordUsageCommand, UsageRecordDto>
{
    private readonly IUsageRecordRepository _usageRecordRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordUsageCommandHandler(IUsageRecordRepository usageRecordRepository, ISubscriptionRepository subscriptionRepository, IUnitOfWork unitOfWork)
    {
        _usageRecordRepository = usageRecordRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UsageRecordDto> Handle(RecordUsageCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.CompanyId, request.SubscriptionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Subscription '{request.SubscriptionId}' was not found.");

        var usageRecord = UsageRecord.Create(request.CompanyId, subscription.Id, request.MetricType, request.Quantity);

        await _usageRecordRepository.AddAsync(usageRecord, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UsageRecordDto(usageRecord.Id, usageRecord.SubscriptionId, usageRecord.MetricType, usageRecord.Quantity, usageRecord.RecordedAt);
    }
}
