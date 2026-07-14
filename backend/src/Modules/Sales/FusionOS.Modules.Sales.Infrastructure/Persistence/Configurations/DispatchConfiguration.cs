using FusionOS.Modules.Sales.Domain.Dispatches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class DispatchConfiguration : IEntityTypeConfiguration<Dispatch>
{
    public void Configure(EntityTypeBuilder<Dispatch> builder)
    {
        builder.ToTable("dispatches");
        builder.HasKey(x => x.Id);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.SalesOrderId });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("DispatchId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
