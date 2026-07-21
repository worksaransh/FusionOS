using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Queries.GetMaintenanceScheduleById;

public sealed class GetMaintenanceScheduleByIdQueryHandler : IRequestHandler<GetMaintenanceScheduleByIdQuery, MaintenanceScheduleDto>
{
    private readonly IMaintenanceScheduleRepository _repository;

    public GetMaintenanceScheduleByIdQueryHandler(IMaintenanceScheduleRepository repository) => _repository = repository;

    public async Task<MaintenanceScheduleDto> Handle(GetMaintenanceScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByIdAsync(request.CompanyId, request.MaintenanceScheduleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance schedule '{request.MaintenanceScheduleId}' was not found.");

        return MaintenanceScheduleMapper.ToDto(schedule);
    }
}
