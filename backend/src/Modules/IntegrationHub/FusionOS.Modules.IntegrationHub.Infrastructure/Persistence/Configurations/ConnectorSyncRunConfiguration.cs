using FusionOS.Modules.IntegrationHub.Domain.ConnectorSyncRuns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Persistence.Configurations;

public sealed class ConnectorSyncRunConfiguration : IEntityTypeConfiguration<ConnectorSyncRun>
{
    public void Configure(EntityTypeBuilder<ConnectorSyncRun> builder)
    {
        builder.ToTable("connector_sync_runs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to ConnectorConnection: existence is validated in the command handler,
        // keeping this reference consistent with every other same-module reference
        // in this codebase (e.g. ConnectorConnection.IntegrationConnectorId).
        builder.HasIndex(x => new { x.CompanyId, x.ConnectorConnectionId });
        builder.HasIndex(x => new { x.CompanyId, x.Status });
    }
}
