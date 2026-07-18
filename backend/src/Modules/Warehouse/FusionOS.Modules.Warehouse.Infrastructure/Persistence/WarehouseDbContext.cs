using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Warehouse.Domain.Bins;
using FusionOS.Modules.Warehouse.Domain.CycleCounts;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using FusionOS.Modules.Warehouse.Domain.PickLists;
using FusionOS.Modules.Warehouse.Domain.Zones;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Persistence;

/// <summary>Owns the "warehouse" schema (04_DATABASE_GUIDELINES.md §1).</summary>
public sealed class WarehouseDbContext : BaseDbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Domain.Warehouses.Warehouse> Warehouses => Set<Domain.Warehouses.Warehouse>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<Bin> Bins => Set<Bin>();
    public DbSet<CycleCount> CycleCounts => Set<CycleCount>();
    public DbSet<PickList> PickLists => Set<PickList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("warehouse");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
