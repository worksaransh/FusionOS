using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Marketplace.Infrastructure.Persistence;

/// <summary>
/// Owns the "marketplace" schema. No entities are mapped yet — this module has not
/// been implemented (reserved for Phase 8 — Marketplace & Ecosystem). Adding the first entity here also
/// means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class MarketplaceDbContext : BaseDbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("marketplace");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketplaceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
