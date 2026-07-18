using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Opportunities.Queries.GetOpportunityById;

public sealed class GetOpportunityByIdQueryHandler : IRequestHandler<GetOpportunityByIdQuery, OpportunityDto>
{
    private readonly IOpportunityRepository _repository;

    public GetOpportunityByIdQueryHandler(IOpportunityRepository repository) => _repository = repository;

    public async Task<OpportunityDto> Handle(GetOpportunityByIdQuery request, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetByIdAsync(request.CompanyId, request.OpportunityId, cancellationToken)
            ?? throw new KeyNotFoundException($"Opportunity '{request.OpportunityId}' was not found.");

        return OpportunityMapper.ToDto(opportunity);
    }
}
