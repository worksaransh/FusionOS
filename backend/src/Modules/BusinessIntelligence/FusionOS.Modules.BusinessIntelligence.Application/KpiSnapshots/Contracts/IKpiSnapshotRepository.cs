namespace FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;

public interface IKpiSnapshotRepository
{
    Task AddAsync(Domain.KpiSnapshots.KpiSnapshot snapshot, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.KpiSnapshots.KpiSnapshot>> ListAsync(Guid companyId, Guid? kpiDefinitionId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? kpiDefinitionId, CancellationToken cancellationToken = default);
}
