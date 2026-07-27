using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Commands.DeactivateInspectionPlan;

public sealed class DeactivateInspectionPlanCommandHandler : IRequestHandler<DeactivateInspectionPlanCommand, InspectionPlanDto>
{
    private readonly IInspectionPlanRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateInspectionPlanCommandHandler(IInspectionPlanRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InspectionPlanDto> Handle(DeactivateInspectionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdAsync(request.CompanyId, request.InspectionPlanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inspection plan '{request.InspectionPlanId}' was not found.");

        plan.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InspectionPlanMapper.ToDto(plan);
    }
}
