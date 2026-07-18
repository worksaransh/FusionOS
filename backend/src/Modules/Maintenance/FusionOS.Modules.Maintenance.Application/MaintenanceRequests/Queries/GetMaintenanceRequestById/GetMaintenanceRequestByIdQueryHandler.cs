using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Queries.GetMaintenanceRequestById;

public sealed class GetMaintenanceRequestByIdQueryHandler : IRequestHandler<GetMaintenanceRequestByIdQuery, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _repository;

    public GetMaintenanceRequestByIdQueryHandler(IMaintenanceRequestRepository repository) => _repository = repository;

    public async Task<MaintenanceRequestDto> Handle(GetMaintenanceRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _repository.GetByIdAsync(request.CompanyId, request.MaintenanceRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance request '{request.MaintenanceRequestId}' was not found.");

        return MaintenanceRequestMapper.ToDto(maintenanceRequest);
    }
}
