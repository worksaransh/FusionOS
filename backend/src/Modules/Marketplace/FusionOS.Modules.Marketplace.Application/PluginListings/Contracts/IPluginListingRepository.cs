namespace FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;

public interface IPluginListingRepository
{
    Task<Domain.PluginListings.PluginListing?> GetByIdAsync(Guid companyId, Guid pluginListingId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid pluginListingId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.PluginListings.PluginListing listing, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.PluginListings.PluginListing>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
