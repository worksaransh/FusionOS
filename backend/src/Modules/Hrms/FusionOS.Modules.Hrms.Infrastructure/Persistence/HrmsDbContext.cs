using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Persistence;

/// <summary>
/// Owns the "hrms" schema. No entities are mapped yet — this module has not
/// been implemented (reserved for Phase 4 — CRM & HRMS). Adding the first entity here also
/// means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class HrmsDbContext : BaseDbContext
{
    public HrmsDbContext(DbContextOptions<HrmsDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("hrms");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
