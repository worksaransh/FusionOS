using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.Settings;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class FinanceSettingsRepository : IFinanceSettingsRepository
{
    private readonly FinanceDbContext _context;

    public FinanceSettingsRepository(FinanceDbContext context) => _context = context;

    public Task<FinanceSettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.FinanceSettings.FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);

    public async Task AddAsync(FinanceSettings settings, CancellationToken cancellationToken = default) =>
        await _context.FinanceSettings.AddAsync(settings, cancellationToken);
}
