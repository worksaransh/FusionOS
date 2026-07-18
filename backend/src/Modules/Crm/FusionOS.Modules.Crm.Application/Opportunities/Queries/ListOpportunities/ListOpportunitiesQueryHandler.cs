using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Opportunities.Queries.ListOpportunities;

public sealed class ListOpportunitiesQueryHandler : IRequestHandler<ListOpportunitiesQuery, PagedResult<OpportunityDto>>
{
    private readonly IOpportunityRepository _repository;

    public ListOpportunitiesQueryHandler(IOpportunityRepository repository) => _repository = repository;

    public async Task<PagedResult<OpportunityDto>> Handle(ListOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var opportunities = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = opportunities.Select(OpportunityMapper.ToDto).ToList();

        return new PagedResult<OpportunityDto>(dtos, request.Page, request.PageSize, total);
    }
}
