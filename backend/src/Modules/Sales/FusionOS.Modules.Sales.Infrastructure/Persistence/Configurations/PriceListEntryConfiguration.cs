using FusionOS.Modules.Sales.Domain.PriceLists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class PriceListEntryConfiguration : IEntityTypeConfiguration<PriceListEntry>
{
    public void Configure(EntityTypeBuilder<PriceListEntry> builder)
    {
        builder.ToTable("price_list_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(19,4)").IsRequired();
    }
}
