using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;

/// <summary>
/// Owns the "integration_hub" schema. First real slice (2026-07-18): the
/// connector catalog (IntegrationConnector) and a company's connections to
/// them (ConnectorConnection) — 05_MODULE_ROADMAP.md's IntegrationHub line
/// item. No credential/secret storage and no real sync engine are mapped
/// here yet — see ConnectorConnection's own class doc comment.
/// </summary>
public sealed class IntegrationHubDbContext : BaseDbContext
{
    public IntegrationHubDbContext(DbContextOptions<IntegrationHubDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<IntegrationConnector> IntegrationConnectors => Set<IntegrationConnector>();
    public DbSet<ConnectorConnection> ConnectorConnections => Set<ConnectorConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("integration_hub");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationHubDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
