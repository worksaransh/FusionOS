using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Commands.CreateInspectionPlan;

public sealed class CreateInspectionPlanCommandHandler : IRequestHandler<CreateInspectionPlanCommand, InspectionPlanDto>
{
    private readonly IInspectionPlanRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInspectionPlanCommandHandler(IInspectionPlanRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InspectionPlanDto> Handle(CreateInspectionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = Domain.InspectionPlans.InspectionPlan.Create(request.CompanyId, request.Name, request.InspectionType, request.CharacteristicNames);

        await _repository.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InspectionPlanMapper.ToDto(plan);
    }
}
