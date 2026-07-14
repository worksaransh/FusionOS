using FusionOS.Modules.Sales.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.TotalAmount);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.SalesOrderId });
        builder.HasIndex(x => new { x.CompanyId, x.CustomerId });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
