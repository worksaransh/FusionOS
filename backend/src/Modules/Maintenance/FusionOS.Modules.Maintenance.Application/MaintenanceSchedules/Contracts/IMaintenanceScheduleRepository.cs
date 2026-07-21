using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;

public interface IMaintenanceScheduleRepository
{
    Task<MaintenanceSchedule?> GetByIdAsync(Guid companyId, Guid maintenanceScheduleId, CancellationToken cancellationToken = default);
    Task AddAsync(MaintenanceSchedule schedule, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceSchedule>> ListAsync(Guid companyId, Guid? assetId, MaintenanceScheduleDueFilter? dueFilter, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? assetId, MaintenanceScheduleDueFilter? dueFilter, CancellationToken cancellationToken = default);
}
