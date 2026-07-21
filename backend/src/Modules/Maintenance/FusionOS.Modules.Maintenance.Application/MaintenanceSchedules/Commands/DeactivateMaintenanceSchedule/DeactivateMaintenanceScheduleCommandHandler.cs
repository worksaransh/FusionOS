using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.DeactivateMaintenanceSchedule;

public sealed class DeactivateMaintenanceScheduleCommandHandler : IRequestHandler<DeactivateMaintenanceScheduleCommand, MaintenanceScheduleDto>
{
    private readonly IMaintenanceScheduleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateMaintenanceScheduleCommandHandler(IMaintenanceScheduleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MaintenanceScheduleDto> Handle(DeactivateMaintenanceScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByIdAsync(request.CompanyId, request.MaintenanceScheduleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Maintenance schedule '{request.MaintenanceScheduleId}' was not found.");

        schedule.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MaintenanceScheduleMapper.ToDto(schedule);
    }
}
