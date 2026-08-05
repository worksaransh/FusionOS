using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using FusionOS.Modules.Billing.Domain.Subscriptions;

namespace FusionOS.Modules.Billing.Application.Bootstrap;

/// <summary>
/// Implements IBillingBootstrapService (FusionOS.BuildingBlocks.Application.Abstractions) —
/// called by Core's RegisterUserCommandHandler, synchronously and in-request, the same way
/// ISegregationOfDutiesService is called from inside another module's handler while its own
/// permission check is already in flight. Deliberately its own IUnitOfWork.SaveChangesAsync
/// call: RegisterUserCommandHandler's unit of work wraps CoreDbContext, not BillingDbContext,
/// so this needs to commit its own write independently (each module owns its own schema and
/// its own DbContext/SaveChanges boundary, per 03_SYSTEM_ARCHITECTURE.md).
/// </summary>
public sealed class BillingBootstrapService : IBillingBootstrapService
{
    private const int DefaultTrialDays = 14;

    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BillingBootstrapService(ISubscriptionRepository subscriptionRepository, IPlanRepository planRepository, IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task EnsureDefaultSubscriptionAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        if (await _subscriptionRepository.ExistsForCompanyAsync(companyId, cancellationToken))
            return;

        var trialPlanId = await _planRepository.GetOrCreateDefaultTrialPlanIdAsync(cancellationToken);
        var subscription = Subscription.Create(companyId, trialPlanId, DateTimeOffset.UtcNow.AddDays(DefaultTrialDays));

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
