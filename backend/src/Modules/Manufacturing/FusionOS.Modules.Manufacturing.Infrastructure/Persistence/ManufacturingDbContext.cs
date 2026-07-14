using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Persistence;

/// <summary>
/// Owns the "manufacturing" schema. No entities are mapped yet — this module has not
/// been implemented (reserved for Phase 3 — Manufacturing ERP). Adding the first entity here also
/// means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class ManufacturingDbContext : BaseDbContext
{
    public ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("manufacturing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManufacturingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
