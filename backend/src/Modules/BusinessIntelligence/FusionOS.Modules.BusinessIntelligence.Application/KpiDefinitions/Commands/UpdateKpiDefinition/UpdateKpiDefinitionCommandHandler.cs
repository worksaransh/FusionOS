using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.UpdateKpiDefinition;

public sealed class UpdateKpiDefinitionCommandHandler : IRequestHandler<UpdateKpiDefinitionCommand, KpiDefinitionDto>
{
    private readonly IKpiDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateKpiDefinitionCommandHandler(IKpiDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<KpiDefinitionDto> Handle(UpdateKpiDefinitionCommand request, CancellationToken cancellationToken)
    {
        var kpi = await _repository.GetByIdAsync(request.CompanyId, request.KpiDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"KPI definition '{request.KpiDefinitionId}' was not found.");

        kpi.UpdateDetails(request.Name, request.Unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return KpiDefinitionMapper.ToDto(kpi);
    }
}
