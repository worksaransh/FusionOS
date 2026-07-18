using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Marketplace.Domain.PluginInstallations;
using FusionOS.Modules.Marketplace.Domain.PluginListings;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Marketplace.Infrastructure.Persistence;

/// <summary>
/// Owns the "marketplace" schema. First real slice (2026-07-18): the
/// extension catalog (PluginListing) and a company's install of one
/// (PluginInstallation) — 05_MODULE_ROADMAP.md's Marketplace line item. No
/// real plugin execution/sandboxing runtime is mapped here yet — see
/// PluginInstallation's own class doc comment.
/// </summary>
public sealed class MarketplaceDbContext : BaseDbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<PluginListing> PluginListings => Set<PluginListing>();
    public DbSet<PluginInstallation> PluginInstallations => Set<PluginInstallation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("marketplace");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketplaceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
