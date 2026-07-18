using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Queries.GetKpiDefinitionById;

public sealed class GetKpiDefinitionByIdQueryHandler : IRequestHandler<GetKpiDefinitionByIdQuery, KpiDefinitionDto>
{
    private readonly IKpiDefinitionRepository _repository;

    public GetKpiDefinitionByIdQueryHandler(IKpiDefinitionRepository repository) => _repository = repository;

    public async Task<KpiDefinitionDto> Handle(GetKpiDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var kpi = await _repository.GetByIdAsync(request.CompanyId, request.KpiDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"KPI definition '{request.KpiDefinitionId}' was not found.");

        return KpiDefinitionMapper.ToDto(kpi);
    }
}
