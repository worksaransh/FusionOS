using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using FusionOS.Modules.Crm.Domain.Opportunities;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class OpportunityRepository : IOpportunityRepository
{
    private readonly CrmDbContext _context;

    public OpportunityRepository(CrmDbContext context) => _context = context;

    public Task<Opportunity?> GetByIdAsync(Guid companyId, Guid opportunityId, CancellationToken cancellationToken = default) =>
        _context.Opportunities.FirstOrDefaultAsync(o => o.CompanyId == companyId && o.Id == opportunityId, cancellationToken);

    public async Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default) =>
        await _context.Opportunities.AddAsync(opportunity, cancellationToken);

    public async Task<IReadOnlyList<Opportunity>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Opportunities
            .Where(o => o.CompanyId == companyId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Opportunities.CountAsync(o => o.CompanyId == companyId, cancellationToken);
}
