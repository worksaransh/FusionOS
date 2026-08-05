using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Application.Subscriptions.Commands.CreateSubscription;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using MediatR;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Commands.ActivateSubscription;

public sealed class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateSubscriptionCommandHandler(ISubscriptionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdAsync(request.CompanyId, request.SubscriptionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Subscription '{request.SubscriptionId}' was not found.");

        subscription.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSubscriptionCommandHandler.MapToDto(subscription);
    }
}
