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
        builder.HasIndex(x => new { x.CompanyId, x.SalesPersonId });

        // GST e-invoicing (IRN/QR) — all null until GenerateEInvoiceCommandHandler records a
        // real (or sandbox) IRP response. Irn is unique per company once set, since an IRP
        // never issues the same IRN twice.
        builder.Property(x => x.Irn).HasMaxLength(64);
        builder.Property(x => x.QrCodePayload).HasMaxLength(2000);
        builder.Property(x => x.IrpAckNumber).HasMaxLength(100);
        builder.HasIndex(x => new { x.CompanyId, x.Irn }).IsUnique().HasFilter("\"Irn\" IS NOT NULL");

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
