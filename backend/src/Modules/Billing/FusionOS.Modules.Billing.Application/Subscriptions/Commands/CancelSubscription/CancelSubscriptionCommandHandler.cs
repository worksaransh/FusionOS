using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Application.Subscriptions.Commands.CreateSubscription;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using MediatR;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Commands.CancelSubscription;

public sealed class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSubscriptionCommandHandler(ISubscriptionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdAsync(request.CompanyId, request.SubscriptionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Subscription '{request.SubscriptionId}' was not found.");

        subscription.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSubscriptionCommandHandler.MapToDto(subscription);
    }
}
