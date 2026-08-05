using FusionOS.Modules.Billing.Application.UsageRecords.Contracts;
using FusionOS.Modules.Billing.Domain.UsageRecords;
using FusionOS.Modules.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Billing.Infrastructure.Repositories;

public sealed class UsageRecordRepository : IUsageRecordRepository
{
    private readonly BillingDbContext _context;

    public UsageRecordRepository(BillingDbContext context) => _context = context;

    public async Task AddAsync(UsageRecord record, CancellationToken cancellationToken = default) =>
        await _context.UsageRecords.AddAsync(record, cancellationToken);

    public async Task<IReadOnlyList<UsageRecord>> ListBySubscriptionAsync(Guid companyId, Guid subscriptionId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.UsageRecords
            .Where(u => u.CompanyId == companyId && u.SubscriptionId == subscriptionId)
            .OrderByDescending(u => u.RecordedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountBySubscriptionAsync(Guid companyId, Guid subscriptionId, CancellationToken cancellationToken = default) =>
        _context.UsageRecords.CountAsync(u => u.CompanyId == companyId && u.SubscriptionId == subscriptionId, cancellationToken);
}
