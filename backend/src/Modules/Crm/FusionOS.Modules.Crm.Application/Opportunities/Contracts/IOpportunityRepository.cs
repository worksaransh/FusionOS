namespace FusionOS.Modules.Crm.Application.Opportunities.Contracts;

public interface IOpportunityRepository
{
    Task<Domain.Opportunities.Opportunity?> GetByIdAsync(Guid companyId, Guid opportunityId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Opportunities.Opportunity opportunity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Opportunities.Opportunity>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
