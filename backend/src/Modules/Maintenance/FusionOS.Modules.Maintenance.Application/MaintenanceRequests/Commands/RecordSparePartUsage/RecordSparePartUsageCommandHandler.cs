using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.RecordSparePartUsage;

public sealed class RecordSparePartUsageCommandHandler : IRequestHandler<RecordSparePartUsageCommand, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordSparePartUsageCommandHandler(IMaintenanceRequestRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MaintenanceRequestDto> Handle(RecordSparePartUsageCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _repository.GetByIdAsync(request.CompanyId, request.MaintenanceRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance request '{request.MaintenanceRequestId}' was not found.");

        maintenanceRequest.RecordSparePartUsage(request.ProductId, request.WarehouseId, request.QuantityUsed, request.UnitCost);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MaintenanceRequestMapper.ToDto(maintenanceRequest);
    }
}
