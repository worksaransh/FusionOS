using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.AssignMaintenanceRequestTechnician;

public sealed class AssignMaintenanceRequestTechnicianCommandHandler : IRequestHandler<AssignMaintenanceRequestTechnicianCommand, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignMaintenanceRequestTechnicianCommandHandler(IMaintenanceRequestRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MaintenanceRequestDto> Handle(AssignMaintenanceRequestTechnicianCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _repository.GetByIdAsync(request.CompanyId, request.MaintenanceRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance request '{request.MaintenanceRequestId}' was not found.");

        maintenanceRequest.AssignTechnician(request.TechnicianUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MaintenanceRequestMapper.ToDto(maintenanceRequest);
    }
}
