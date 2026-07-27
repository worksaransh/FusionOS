namespace FusionOS.Modules.Finance.Application.TaxRates.Contracts;

public interface ITaxRateRepository
{
    Task<Domain.TaxRates.TaxRate?> GetByIdAsync(Guid companyId, Guid taxRateId, CancellationToken cancellationToken = default);
    Task<bool> TaxJurisdictionExistsAsync(Guid companyId, Guid taxJurisdictionId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, Guid taxJurisdictionId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.TaxRates.TaxRate taxRate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.TaxRates.TaxRate>> ListAsync(Guid companyId, Guid taxJurisdictionId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid taxJurisdictionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every active TaxRate for the company, across all jurisdictions, unpaged —
    /// backs GetTaxPeriodSummaryReportQuery, which needs the whole active rate
    /// register in one shot rather than one jurisdiction's page at a time.
    /// </summary>
    Task<IReadOnlyList<Domain.TaxRates.TaxRate>> ListAllActiveAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk resolves the given TaxRate ids (active or not — a rate that has since
    /// been deactivated must still resolve for a historical transaction) in one
    /// round trip — backs GetGstReturnSummaryReportQueryHandler, which needs the
    /// GstComponent for every distinct TaxRateId referenced across a period's
    /// TaxTransactions rather than one GetByIdAsync call per distinct rate. Same
    /// "batch-resolve by id set" convention as Crm's ITagRepository.GetByIdsAsync.
    /// </summary>
    Task<IReadOnlyList<Domain.TaxRates.TaxRate>> GetByIdsAsync(Guid companyId, IReadOnlyCollection<Guid> taxRateIds, CancellationToken cancellationToken = default);
}
