using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Ai.Domain.Copilot;
using FusionOS.Modules.Ai.Domain.DemandHistory;
using FusionOS.Modules.Ai.Domain.OcrJobs;
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

    /// <summary>Phase 8 — AI &amp; Integrations. See OcrJob's own class doc comment for what this pipeline is and is not.</summary>
    public DbSet<OcrJob> OcrJobs => Set<OcrJob>();

    /// <summary>Phase 8 — AI &amp; Integrations. See DemandHistoryEntry's own class doc comment: the real, event-fed demand journal GetDemandForecastQuery's linear regression/moving average computes over.</summary>
    public DbSet<DemandHistoryEntry> DemandHistoryEntries => Set<DemandHistoryEntry>();

    /// <summary>Phase 8 — AI Copilot. See CopilotSession's own class doc comment for what this persists and doesn't.</summary>
    public DbSet<CopilotSession> CopilotSessions => Set<CopilotSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ai");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
