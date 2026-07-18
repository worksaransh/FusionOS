using FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Persistence.Configurations;

public sealed class ConnectorConnectionConfiguration : IEntityTypeConfiguration<ConnectorConnection>
{
    public void Configure(EntityTypeBuilder<ConnectorConnection> builder)
    {
        builder.ToTable("connector_connections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to IntegrationConnector: existence is validated in the command handler,
        // keeping this reference consistent with every other same-module reference
        // in this codebase (e.g. BudgetLine.AccountId, PluginInstallation.PluginListingId).
        builder.HasIndex(x => new { x.CompanyId, x.IntegrationConnectorId });
        builder.HasIndex(x => new { x.CompanyId, x.Status });
    }
}
