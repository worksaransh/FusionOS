using FusionOS.Modules.Procurement.Domain.Rfqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

public sealed class RfqConfiguration : IEntityTypeConfiguration<RequestForQuotation>
{
    public void Configure(EntityTypeBuilder<RequestForQuotation> builder)
    {
        builder.ToTable("rfqs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => x.CompanyId);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("RfqId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.SupplierQuotes)
            .WithOne()
            .HasForeignKey("RfqId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.SupplierQuotes).HasField("_supplierQuotes").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
