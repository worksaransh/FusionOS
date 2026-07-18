using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;
using FusionOS.Modules.Marketplace.Domain.PluginInstallations;
using FusionOS.Modules.Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Marketplace.Infrastructure.Repositories;

public sealed class PluginInstallationRepository : IPluginInstallationRepository
{
    private readonly MarketplaceDbContext _context;

    public PluginInstallationRepository(MarketplaceDbContext context) => _context = context;

    public Task<PluginInstallation?> GetByIdAsync(Guid companyId, Guid pluginInstallationId, CancellationToken cancellationToken = default) =>
        _context.PluginInstallations.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Id == pluginInstallationId, cancellationToken);

    public async Task AddAsync(PluginInstallation installation, CancellationToken cancellationToken = default) =>
        await _context.PluginInstallations.AddAsync(installation, cancellationToken);

    public async Task<IReadOnlyList<PluginInstallation>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.PluginInstallations
            .Where(p => p.CompanyId == companyId)
            .OrderByDescending(p => p.InstalledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.PluginInstallations.CountAsync(p => p.CompanyId == companyId, cancellationToken);
}
