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

    public async Task<IReadOnlyList<(OpportunityStage Stage, decimal EstimatedValueTotal, decimal WeightedValueTotal)>> GetOpenPipelineByStageAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var grouped = await _context.Opportunities
            .Where(o => o.CompanyId == companyId && o.Stage != OpportunityStage.Won && o.Stage != OpportunityStage.Lost)
            .GroupBy(o => o.Stage)
            .Select(g => new
            {
                Stage = g.Key,
                EstimatedValueTotal = g.Sum(o => o.EstimatedValue),
                WeightedValueTotal = g.Sum(o => o.EstimatedValue * (o.Probability ?? 100m) / 100m),
            })
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.Stage, g.EstimatedValueTotal, g.WeightedValueTotal)).ToList();
    }

    public async Task<(int WonCount, int LostCount)> GetClosedCountsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var wonCount = await _context.Opportunities.CountAsync(o => o.CompanyId == companyId && o.Stage == OpportunityStage.Won, cancellationToken);
        var lostCount = await _context.Opportunities.CountAsync(o => o.CompanyId == companyId && o.Stage == OpportunityStage.Lost, cancellationToken);
        return (wonCount, lostCount);
    }
}
