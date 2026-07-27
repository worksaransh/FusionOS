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
    // Added for QualityAudit/DeviationRequest (2026-07-23) — AuditFinding has no DbSet of its
    // own, same "owned by parent, no independent querying" convention as InspectionItem.
    public DbSet<FusionOS.Modules.Quality.Domain.Audits.QualityAudit> QualityAudits => Set<FusionOS.Modules.Quality.Domain.Audits.QualityAudit>();
    public DbSet<FusionOS.Modules.Quality.Domain.Deviations.DeviationRequest> DeviationRequests => Set<FusionOS.Modules.Quality.Domain.Deviations.DeviationRequest>();
    public DbSet<FusionOS.Modules.Quality.Domain.InspectionPlans.InspectionPlan> InspectionPlans => Set<FusionOS.Modules.Quality.Domain.InspectionPlans.InspectionPlan>();
    public DbSet<FusionOS.Modules.Quality.Domain.QualityHolds.QualityHold> QualityHolds => Set<FusionOS.Modules.Quality.Domain.QualityHolds.QualityHold>();
    public DbSet<FusionOS.Modules.Quality.Domain.Calibrations.CalibrationRecord> CalibrationRecords => Set<FusionOS.Modules.Quality.Domain.Calibrations.CalibrationRecord>();
    public DbSet<FusionOS.Modules.Quality.Domain.Certificates.QualityCertificate> QualityCertificates => Set<FusionOS.Modules.Quality.Domain.Certificates.QualityCertificate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("quality");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QualityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
