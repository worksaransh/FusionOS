using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Ai.Domain.Recommendations;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Ai.Infrastructure.Persistence;

/// <summary>
/// Owns the "ai" schema. First real slice (2026-07-18): Recommendation, the
/// durable, human-in-the-loop record `docs/blueprint/12_AI_PLATFORM.md` §3
/// describes as this module's ".NET AI orchestration layer" — see
/// Recommendation's own class doc comment for why no real forecasting/OCR/ML
/// model is mapped here yet.
/// </summary>
public sealed class AiDbContext : BaseDbContext
{
    public AiDbContext(DbContextOptions<AiDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ai");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
