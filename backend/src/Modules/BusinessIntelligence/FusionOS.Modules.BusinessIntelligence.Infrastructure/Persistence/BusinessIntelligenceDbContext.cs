using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiDefinitions;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.BusinessIntelligence.Infrastructure.Persistence;

/// <summary>
/// Owns the "bi" schema. First real slice (2026-07-18): a KPI catalog
/// (KpiDefinition) and manually-recorded point-in-time values against it
/// (KpiSnapshot) — 05_MODULE_ROADMAP.md's "KPIs"/"Dashboards"/"Charts" line
/// items. No automated cross-module ingestion pipeline exists yet — see
/// KpiDefinition's own class doc comment for why that is deliberately out of
/// scope here rather than half-wired.
/// </summary>
public sealed class BusinessIntelligenceDbContext : BaseDbContext
{
    public BusinessIntelligenceDbContext(DbContextOptions<BusinessIntelligenceDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<KpiDefinition> KpiDefinitions => Set<KpiDefinition>();
    public DbSet<KpiSnapshot> KpiSnapshots => Set<KpiSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("bi");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusinessIntelligenceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
