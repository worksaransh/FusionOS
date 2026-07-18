using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Maintenance.Domain.Assets;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Maintenance.Infrastructure.Persistence;

/// <summary>
/// Owns the "maintenance" schema. First real slice (2026-07-18): the machine
/// register (Asset) and preventive/breakdown maintenance requests against it
/// (MaintenanceRequest) — 05_MODULE_ROADMAP.md's Maintenance line item.
/// Spare parts tracking is not yet mapped here — a separately-scoped follow-up.
/// </summary>
public sealed class MaintenanceDbContext : BaseDbContext
{
    public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("maintenance");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaintenanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
