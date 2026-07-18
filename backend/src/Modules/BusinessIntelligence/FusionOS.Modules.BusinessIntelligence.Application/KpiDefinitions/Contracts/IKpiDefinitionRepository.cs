namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;

public interface IKpiDefinitionRepository
{
    Task<Domain.KpiDefinitions.KpiDefinition?> GetByIdAsync(Guid companyId, Guid kpiDefinitionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid kpiDefinitionId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.KpiDefinitions.KpiDefinition kpiDefinition, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.KpiDefinitions.KpiDefinition>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
