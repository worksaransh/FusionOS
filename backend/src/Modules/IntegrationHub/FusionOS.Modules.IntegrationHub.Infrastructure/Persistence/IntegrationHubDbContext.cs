using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorSyncRuns;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;

/// <summary>
/// Owns the "integration_hub" schema. First real slice (2026-07-18): the
/// connector catalog (IntegrationConnector) and a company's connections to
/// them (ConnectorConnection) — 05_MODULE_ROADMAP.md's IntegrationHub line
/// item.
///
/// Phase 8 (2026-07-25) added <see cref="ConnectorConnections.ConnectorConnection.CredentialReference"/>
/// (still never a secret value, see that property's doc comment) and
/// <see cref="ConnectorSyncRuns.ConnectorSyncRun"/> — the first real sync run history,
/// populated by TriggerConnectorSyncCommand and the Shopify webhook receiver.
/// </summary>
public sealed class IntegrationHubDbContext : BaseDbContext
{
    public IntegrationHubDbContext(DbContextOptions<IntegrationHubDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<IntegrationConnector> IntegrationConnectors => Set<IntegrationConnector>();
    public DbSet<ConnectorConnection> ConnectorConnections => Set<ConnectorConnection>();
    public DbSet<MigrationJob> MigrationJobs => Set<MigrationJob>();
    public DbSet<MigrationStagingRecord> MigrationStagingRecords => Set<MigrationStagingRecord>();
    public DbSet<ConnectorSyncRun> ConnectorSyncRuns => Set<ConnectorSyncRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("integration_hub");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationHubDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
