using FusionOS.Modules.Billing.Application.Subscriptions.Commands.CreateSubscription;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using MediatR;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Queries.GetSubscriptionByCompany;

public sealed class GetSubscriptionByCompanyQueryHandler : IRequestHandler<GetSubscriptionByCompanyQuery, SubscriptionDto?>
{
    private readonly ISubscriptionRepository _repository;

    public GetSubscriptionByCompanyQueryHandler(ISubscriptionRepository repository) => _repository = repository;

    public async Task<SubscriptionDto?> Handle(GetSubscriptionByCompanyQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);
        return subscription is null ? null : CreateSubscriptionCommandHandler.MapToDto(subscription);
    }
}
