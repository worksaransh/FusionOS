using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;

/// <summary>
/// Owns the "integration_hub" schema. No entities are mapped yet — this module has not
/// been implemented (reserved for Phase 9 — Integrations & Mobile). Adding the first entity here also
/// means adding its IEntityTypeConfiguration and an EF Core migration, per
/// docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class IntegrationHubDbContext : BaseDbContext
{
    public IntegrationHubDbContext(DbContextOptions<IntegrationHubDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("integration_hub");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationHubDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
