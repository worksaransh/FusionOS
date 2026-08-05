using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using FusionOS.Modules.Billing.Domain.Subscriptions;
using FusionOS.Modules.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Billing.Infrastructure.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly BillingDbContext _context;

    public SubscriptionRepository(BillingDbContext context) => _context = context;

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default) =>
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);

    public Task<Subscription?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.Subscriptions.FirstOrDefaultAsync(s => s.CompanyId == companyId && s.Id == id, cancellationToken);

    public Task<Subscription?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Subscriptions.FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);

    public Task<bool> ExistsForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Subscriptions.AnyAsync(s => s.CompanyId == companyId, cancellationToken);
}
