using FusionOS.Modules.Sales.Domain.EWayBills;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class EWayBillConfiguration : IEntityTypeConfiguration<EWayBill>
{
    public void Configure(EntityTypeBuilder<EWayBill> builder)
    {
        builder.ToTable("e_way_bills");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EwbNumber).HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.DispatchId }).IsUnique(); // one e-way bill per dispatch
    }
}
