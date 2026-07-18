using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CompleteMaintenanceRequest;

public sealed class CompleteMaintenanceRequestCommandHandler : IRequestHandler<CompleteMaintenanceRequestCommand, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteMaintenanceRequestCommandHandler(IMaintenanceRequestRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MaintenanceRequestDto> Handle(CompleteMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _repository.GetByIdAsync(request.CompanyId, request.MaintenanceRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance request '{request.MaintenanceRequestId}' was not found.");

        maintenanceRequest.Complete(request.ResolutionNotes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MaintenanceRequestMapper.ToDto(maintenanceRequest);
    }
}
