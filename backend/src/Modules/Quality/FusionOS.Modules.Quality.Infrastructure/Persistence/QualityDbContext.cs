using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence;

/// <summary>
/// Owns the "quality" schema. Phase 5 — Quality first slice: Inspections. Adding an entity
/// here also means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class QualityDbContext : BaseDbContext
{
    public QualityDbContext(DbContextOptions<QualityDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<FusionOS.Modules.Quality.Domain.Inspections.Inspection> Inspections => Set<FusionOS.Modules.Quality.Domain.Inspections.Inspection>();
    public DbSet<FusionOS.Modules.Quality.Domain.NonConformanceReports.NonConformanceReport> NonConformanceReports => Set<FusionOS.Modules.Quality.Domain.NonConformanceReports.NonConformanceReport>();
    public DbSet<FusionOS.Modules.Quality.Domain.CorrectiveActions.CorrectiveAction> CorrectiveActions => Set<FusionOS.Modules.Quality.Domain.CorrectiveActions.CorrectiveAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("quality");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QualityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
