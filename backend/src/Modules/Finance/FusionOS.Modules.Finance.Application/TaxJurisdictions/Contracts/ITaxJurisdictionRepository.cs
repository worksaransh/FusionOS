namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;

public interface ITaxJurisdictionRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<Domain.TaxJurisdictions.TaxJurisdiction?> GetByIdAsync(Guid companyId, Guid taxJurisdictionId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.TaxJurisdictions.TaxJurisdiction taxJurisdiction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.TaxJurisdictions.TaxJurisdiction>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
