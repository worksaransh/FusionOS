using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Maintenance.Infrastructure.Persistence;

/// <summary>
/// Owns the "maintenance" schema. No entities are mapped yet — this module has not
/// been implemented (reserved for Phase 5 — Quality & Maintenance). Adding the first entity here also
/// means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class MaintenanceDbContext : BaseDbContext
{
    public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("maintenance");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaintenanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
