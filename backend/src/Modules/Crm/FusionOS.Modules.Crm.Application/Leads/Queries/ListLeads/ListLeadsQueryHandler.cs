using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Queries.ListLeads;

public sealed class ListLeadsQueryHandler : IRequestHandler<ListLeadsQuery, PagedResult<LeadDto>>
{
    private readonly ILeadRepository _repository;

    public ListLeadsQueryHandler(ILeadRepository repository) => _repository = repository;

    public async Task<PagedResult<LeadDto>> Handle(ListLeadsQuery request, CancellationToken cancellationToken)
    {
        var leads = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = leads.Select(LeadMapper.ToDto).ToList();

        return new PagedResult<LeadDto>(dtos, request.Page, request.PageSize, total);
    }
}
