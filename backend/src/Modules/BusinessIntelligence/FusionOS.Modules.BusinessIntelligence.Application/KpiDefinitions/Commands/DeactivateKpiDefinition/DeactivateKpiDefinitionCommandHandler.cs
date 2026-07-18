using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.DeactivateKpiDefinition;

public sealed class DeactivateKpiDefinitionCommandHandler : IRequestHandler<DeactivateKpiDefinitionCommand, KpiDefinitionDto>
{
    private readonly IKpiDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateKpiDefinitionCommandHandler(IKpiDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<KpiDefinitionDto> Handle(DeactivateKpiDefinitionCommand request, CancellationToken cancellationToken)
    {
        var kpi = await _repository.GetByIdAsync(request.CompanyId, request.KpiDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"KPI definition '{request.KpiDefinitionId}' was not found.");

        kpi.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return KpiDefinitionMapper.ToDto(kpi);
    }
}
