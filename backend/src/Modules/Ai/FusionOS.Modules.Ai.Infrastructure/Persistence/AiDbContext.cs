using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Ai.Infrastructure.Persistence;

/// <summary>
/// Owns the "ai" schema. No entities are mapped yet — this module has not
/// been implemented (reserved for Phase 7 — AI Platform). Adding the first entity here also
/// means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class AiDbContext : BaseDbContext
{
    public AiDbContext(DbContextOptions<AiDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ai");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
