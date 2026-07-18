namespace FusionOS.Modules.Finance.Application.CostCenters.Contracts;

public interface ICostCenterRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<Domain.CostCenters.CostCenter?> GetByIdAsync(Guid companyId, Guid costCenterId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.CostCenters.CostCenter costCenter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.CostCenters.CostCenter>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
