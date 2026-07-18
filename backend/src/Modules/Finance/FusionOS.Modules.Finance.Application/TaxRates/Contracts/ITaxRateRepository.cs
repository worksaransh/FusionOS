namespace FusionOS.Modules.Finance.Application.TaxRates.Contracts;

public interface ITaxRateRepository
{
    Task<Domain.TaxRates.TaxRate?> GetByIdAsync(Guid companyId, Guid taxRateId, CancellationToken cancellationToken = default);
    Task<bool> TaxJurisdictionExistsAsync(Guid companyId, Guid taxJurisdictionId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, Guid taxJurisdictionId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.TaxRates.TaxRate taxRate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.TaxRates.TaxRate>> ListAsync(Guid companyId, Guid taxJurisdictionId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid taxJurisdictionId, CancellationToken cancellationToken = default);
}
