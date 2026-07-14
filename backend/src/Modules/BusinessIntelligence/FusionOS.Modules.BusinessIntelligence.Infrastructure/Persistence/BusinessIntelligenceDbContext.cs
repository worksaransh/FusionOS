using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.BusinessIntelligence.Infrastructure.Persistence;

/// <summary>
/// Owns the "bi" schema. No entities are mapped yet — this module has not
/// been implemented (reserved for Phase 6 — Business Intelligence). Adding the first entity here also
/// means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class BusinessIntelligenceDbContext : BaseDbContext
{
    public BusinessIntelligenceDbContext(DbContextOptions<BusinessIntelligenceDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("bi");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusinessIntelligenceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
