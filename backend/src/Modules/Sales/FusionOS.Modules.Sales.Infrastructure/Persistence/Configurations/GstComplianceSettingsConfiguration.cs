using FusionOS.Modules.Sales.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class GstComplianceSettingsConfiguration : IEntityTypeConfiguration<GstComplianceSettings>
{
    public void Configure(EntityTypeBuilder<GstComplianceSettings> builder)
    {
        builder.ToTable("gst_compliance_settings");
        builder.HasKey(x => x.Id);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => x.CompanyId).IsUnique(); // one row per company — get-or-create enforcement
    }
}
