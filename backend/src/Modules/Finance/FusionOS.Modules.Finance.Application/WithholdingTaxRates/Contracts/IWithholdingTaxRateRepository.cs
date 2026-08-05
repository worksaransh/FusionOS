namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;

public interface IWithholdingTaxRateRepository
{
    Task<Domain.WithholdingTaxRates.WithholdingTaxRate?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.WithholdingTaxRates.WithholdingTaxRate rate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.WithholdingTaxRates.WithholdingTaxRate>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> SectionCodeExistsAsync(Guid companyId, string sectionCode, CancellationToken cancellationToken = default);
}
