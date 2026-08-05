using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using FusionOS.Modules.Billing.Domain.Subscriptions;
using MediatR;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepository, IPlanRepository planRepository, IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plan '{request.PlanId}' was not found.");

        var trialEndsAt = request.TrialDays is { } days ? DateTimeOffset.UtcNow.AddDays(days) : (DateTimeOffset?)null;
        var subscription = Subscription.Create(request.CompanyId, plan.Id, trialEndsAt);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(subscription);
    }

    internal static SubscriptionDto MapToDto(Subscription subscription) => new(
        subscription.Id, subscription.CompanyId, subscription.PlanId, subscription.Status.ToString(),
        subscription.StartedAt, subscription.TrialEndsAt, subscription.CancelledAt);
}
