using FusionOS.Modules.Procurement.Domain.Rfqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

/// <summary>See RfqLine's doc comment for why this table has no audit/tenant columns of its own, and no price column.</summary>
public sealed class RfqLineConfiguration : IEntityTypeConfiguration<RfqLine>
{
    public void Configure(EntityTypeBuilder<RfqLine> builder)
    {
        builder.ToTable("rfq_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("numeric(19,4)").IsRequired();
    }
}
