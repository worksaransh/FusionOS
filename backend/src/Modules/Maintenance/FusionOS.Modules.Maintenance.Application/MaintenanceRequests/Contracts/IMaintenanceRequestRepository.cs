namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

public interface IMaintenanceRequestRepository
{
    Task<Domain.MaintenanceRequests.MaintenanceRequest?> GetByIdAsync(Guid companyId, Guid maintenanceRequestId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.MaintenanceRequests.MaintenanceRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.MaintenanceRequests.MaintenanceRequest>> ListAsync(Guid companyId, Guid? assetId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? assetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every Completed request (Preventive or Breakdown) whose completion (CompletedAt)
    /// falls within [periodStart, periodEnd], optionally scoped to one asset — backs
    /// GetDowntimeMetricsReportQueryHandler's MTTR/MTBF computation. Only Completed
    /// requests are returned since ActualDowntimeMinutes and CompletedAt are only ever
    /// set by MaintenanceRequest.Complete(); an in-progress or open request contributes
    /// no data point here.
    /// </summary>
    Task<IReadOnlyList<Domain.MaintenanceRequests.MaintenanceRequest>> ListCompletedInPeriodAsync(Guid companyId, Guid? assetId, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default);
}
