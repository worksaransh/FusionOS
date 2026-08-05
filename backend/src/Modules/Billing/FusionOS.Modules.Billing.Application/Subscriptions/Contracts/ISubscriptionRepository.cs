using FusionOS.Modules.Billing.Domain.Subscriptions;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Contracts;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
}
