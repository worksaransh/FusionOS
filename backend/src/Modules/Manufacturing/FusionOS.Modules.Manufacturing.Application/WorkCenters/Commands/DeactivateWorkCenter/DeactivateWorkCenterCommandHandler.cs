using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.DeactivateWorkCenter;

public sealed class DeactivateWorkCenterCommandHandler : IRequestHandler<DeactivateWorkCenterCommand, WorkCenterDto>
{
    private readonly IWorkCenterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateWorkCenterCommandHandler(IWorkCenterRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkCenterDto> Handle(DeactivateWorkCenterCommand request, CancellationToken cancellationToken)
    {
        var workCenter = await _repository.GetByIdAsync(request.CompanyId, request.WorkCenterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work center '{request.WorkCenterId}' was not found.");

        workCenter.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkCenterMapper.ToDto(workCenter);
    }
}
