using FusionOS.Modules.Crm.Domain.Opportunities;

namespace FusionOS.Modules.Crm.Application.Opportunities.Contracts;

public interface IOpportunityRepository
{
    Task<Domain.Opportunities.Opportunity?> GetByIdAsync(Guid companyId, Guid opportunityId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Opportunities.Opportunity opportunity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Opportunities.Opportunity>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Server-side (GroupBy, no full row materialization) sum of EstimatedValue and the
    /// weighted equivalent (EstimatedValue * Probability/100, defaulting the multiplier to
    /// 100 when Probability is null) per non-terminal stage, for the CRM dashboard's
    /// pipeline-by-stage breakdown. Only stages with at least one open opportunity are
    /// returned — the handler fills in zero for any stage with none.
    /// </summary>
    Task<IReadOnlyList<(OpportunityStage Stage, decimal EstimatedValueTotal, decimal WeightedValueTotal)>> GetOpenPipelineByStageAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Server-side counts of Won vs Lost opportunities (the only two closed stages) for the CRM dashboard's win-rate metric.</summary>
    Task<(int WonCount, int LostCount)> GetClosedCountsAsync(Guid companyId, CancellationToken cancellationToken = default);
}
