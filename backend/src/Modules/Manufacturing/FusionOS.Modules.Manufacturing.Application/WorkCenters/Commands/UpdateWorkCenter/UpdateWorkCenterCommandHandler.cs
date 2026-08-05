using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.UpdateWorkCenter;

public sealed class UpdateWorkCenterCommandHandler : IRequestHandler<UpdateWorkCenterCommand, WorkCenterDto>
{
    private readonly IWorkCenterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWorkCenterCommandHandler(IWorkCenterRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkCenterDto> Handle(UpdateWorkCenterCommand request, CancellationToken cancellationToken)
    {
        var workCenter = await _repository.GetByIdAsync(request.CompanyId, request.WorkCenterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work center '{request.WorkCenterId}' was not found.");

        workCenter.UpdateDetails(request.Name, request.CapacityHoursPerDay);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkCenterMapper.ToDto(workCenter);
    }
}
