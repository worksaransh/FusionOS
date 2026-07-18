using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.CreateKpiDefinition;

public sealed class CreateKpiDefinitionCommandHandler : IRequestHandler<CreateKpiDefinitionCommand, KpiDefinitionDto>
{
    private readonly IKpiDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateKpiDefinitionCommandHandler(IKpiDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<KpiDefinitionDto> Handle(CreateKpiDefinitionCommand request, CancellationToken cancellationToken)
    {
        var kpi = Domain.KpiDefinitions.KpiDefinition.Create(request.CompanyId, request.Code, request.Name, request.Unit);

        await _repository.AddAsync(kpi, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return KpiDefinitionMapper.ToDto(kpi);
    }
}
