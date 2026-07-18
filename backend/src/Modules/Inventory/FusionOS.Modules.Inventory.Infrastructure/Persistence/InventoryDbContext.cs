using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.Modules.Inventory.Domain.Products;
using FusionOS.Modules.Inventory.Domain.Reservations;
using FusionOS.Modules.Inventory.Domain.Transfers;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Persistence;

/// <summary>Owns the "inventory" schema (04_DATABASE_GUIDELINES.md §1).</summary>
public sealed class InventoryDbContext : BaseDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryLedgerEntry> LedgerEntries => Set<InventoryLedgerEntry>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
