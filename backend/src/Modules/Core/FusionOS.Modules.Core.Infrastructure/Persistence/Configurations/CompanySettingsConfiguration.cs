using FusionOS.Modules.Core.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.ToTable("company_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.DefaultCurrency).HasMaxLength(3).IsRequired();
        builder.Property(s => s.DisplayName).HasMaxLength(200);
        builder.Property(s => s.LogoUrl).HasMaxLength(2000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(s => s.RowVersion);
        builder.HasIndex(s => s.CompanyId).IsUnique(); // one settings row per company
        builder.Ignore(s => s.DomainEvents);
    }
}
