namespace FusionOS.Modules.Core.Application.Settings.Contracts;

public interface ICompanySettingsRepository
{
    Task<Domain.Settings.CompanySettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.Settings.CompanySettings settings, CancellationToken cancellationToken = default);
}
