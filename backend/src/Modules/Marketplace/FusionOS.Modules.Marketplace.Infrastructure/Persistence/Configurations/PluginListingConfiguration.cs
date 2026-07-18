using FusionOS.Modules.Marketplace.Domain.PluginListings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Marketplace.Infrastructure.Persistence.Configurations;

public sealed class PluginListingConfiguration : IEntityTypeConfiguration<PluginListing>
{
    public void Configure(EntityTypeBuilder<PluginListing> builder)
    {
        builder.ToTable("plugin_listings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Publisher).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
    }
}
