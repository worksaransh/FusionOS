namespace FusionOS.Modules.Finance.Application.Settings.Contracts;

public interface IFinanceSettingsRepository
{
    Task<Domain.Settings.FinanceSettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Settings.FinanceSettings settings, CancellationToken cancellationToken = default);
}
