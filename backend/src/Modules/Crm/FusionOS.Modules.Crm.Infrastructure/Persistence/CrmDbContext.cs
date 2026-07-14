using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Persistence;

/// <summary>
/// Owns the "crm" schema. No entities are mapped yet — this module has not
/// been implemented (reserved for Phase 4 — CRM & HRMS). Adding the first entity here also
/// means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class CrmDbContext : BaseDbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("crm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
