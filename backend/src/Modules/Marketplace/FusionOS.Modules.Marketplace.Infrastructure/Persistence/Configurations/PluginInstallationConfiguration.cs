using FusionOS.Modules.Marketplace.Domain.PluginInstallations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Marketplace.Infrastructure.Persistence.Configurations;

public sealed class PluginInstallationConfiguration : IEntityTypeConfiguration<PluginInstallation>
{
    public void Configure(EntityTypeBuilder<PluginInstallation> builder)
    {
        builder.ToTable("plugin_installations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to PluginListing: existence is validated in the command handler,
        // keeping this reference consistent with every other same-module reference
        // in this codebase (e.g. BudgetLine.AccountId, LeaveRequest.EmployeeId).
        builder.HasIndex(x => new { x.CompanyId, x.PluginListingId });
        builder.HasIndex(x => new { x.CompanyId, x.Status });
    }
}
