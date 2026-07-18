using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Persistence;

/// <summary>
/// Owns the "manufacturing" schema. Phase 3 — Manufacturing ERP first slice: Bills of
/// Materials and Work Orders. Adding an entity here also means adding its
/// IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class ManufacturingDbContext : BaseDbContext
{
    public ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<FusionOS.Modules.Manufacturing.Domain.BillOfMaterials.BillOfMaterials> BillsOfMaterials => Set<FusionOS.Modules.Manufacturing.Domain.BillOfMaterials.BillOfMaterials>();
    public DbSet<FusionOS.Modules.Manufacturing.Domain.WorkOrders.WorkOrder> WorkOrders => Set<FusionOS.Modules.Manufacturing.Domain.WorkOrders.WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("manufacturing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManufacturingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
