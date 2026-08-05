using FusionOS.Modules.Sales.Application.Settings.Contracts;
using FusionOS.Modules.Sales.Domain.Settings;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class GstComplianceSettingsRepository : IGstComplianceSettingsRepository
{
    private readonly SalesDbContext _context;

    public GstComplianceSettingsRepository(SalesDbContext context) => _context = context;

    public Task<GstComplianceSettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.GstComplianceSettings.FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);

    public async Task AddAsync(GstComplianceSettings settings, CancellationToken cancellationToken = default) =>
        await _context.GstComplianceSettings.AddAsync(settings, cancellationToken);
}
