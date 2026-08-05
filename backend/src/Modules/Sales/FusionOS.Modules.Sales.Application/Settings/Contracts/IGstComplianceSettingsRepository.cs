namespace FusionOS.Modules.Sales.Application.Settings.Contracts;

public interface IGstComplianceSettingsRepository
{
    Task<Domain.Settings.GstComplianceSettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Settings.GstComplianceSettings settings, CancellationToken cancellationToken = default);
}
