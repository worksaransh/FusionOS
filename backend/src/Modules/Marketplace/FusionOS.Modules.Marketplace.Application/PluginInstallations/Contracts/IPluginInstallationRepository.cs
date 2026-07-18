namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;

public interface IPluginInstallationRepository
{
    Task<Domain.PluginInstallations.PluginInstallation?> GetByIdAsync(Guid companyId, Guid pluginInstallationId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.PluginInstallations.PluginInstallation installation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.PluginInstallations.PluginInstallation>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
