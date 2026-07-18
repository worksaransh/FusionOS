namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

public interface IMaintenanceRequestRepository
{
    Task<Domain.MaintenanceRequests.MaintenanceRequest?> GetByIdAsync(Guid companyId, Guid maintenanceRequestId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.MaintenanceRequests.MaintenanceRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.MaintenanceRequests.MaintenanceRequest>> ListAsync(Guid companyId, Guid? assetId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? assetId, CancellationToken cancellationToken = default);
}
