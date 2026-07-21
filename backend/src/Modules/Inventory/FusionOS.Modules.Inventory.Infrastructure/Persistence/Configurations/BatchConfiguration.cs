using FusionOS.Modules.Inventory.Domain.Batches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("batches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.QuantityReceived).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.QuantityRemaining).HasColumnType("numeric(19,4)").IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.BatchNumber }).IsUnique();
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.ExpiryDate }); // ListBatchesByProductQuery's ExpiringBefore lookup shape
    }
}
