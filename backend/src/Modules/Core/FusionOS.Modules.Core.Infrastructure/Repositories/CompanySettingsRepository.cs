using FusionOS.Modules.Core.Application.Settings.Contracts;
using FusionOS.Modules.Core.Domain.Settings;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class CompanySettingsRepository : ICompanySettingsRepository
{
    private readonly CoreDbContext _context;

    public CompanySettingsRepository(CoreDbContext context) => _context = context;

    public Task<CompanySettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.CompanySettings.FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);

    public async Task AddAsync(CompanySettings settings, CancellationToken cancellationToken = default) =>
        await _context.CompanySettings.AddAsync(settings, cancellationToken);
}
