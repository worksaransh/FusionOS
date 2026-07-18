using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Queries.ListKpiDefinitions;

public sealed class ListKpiDefinitionsQueryHandler : IRequestHandler<ListKpiDefinitionsQuery, PagedResult<KpiDefinitionDto>>
{
    private readonly IKpiDefinitionRepository _repository;

    public ListKpiDefinitionsQueryHandler(IKpiDefinitionRepository repository) => _repository = repository;

    public async Task<PagedResult<KpiDefinitionDto>> Handle(ListKpiDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var kpis = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = kpis.Select(KpiDefinitionMapper.ToDto).ToList();

        return new PagedResult<KpiDefinitionDto>(dtos, request.Page, request.PageSize, total);
    }
}
