using FusionOS.Modules.Sales.Domain.Dispatches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fixes a confirmed audit gap (2026-07 sprint): DispatchLine's decimal column had
/// no explicit precision/scale configured, so SQL Server/Postgres would otherwise
/// fall back to a default that silently truncates fractional values. Matches the
/// numeric(19,4) convention established by JournalEntryLine/SalesOrderLine/
/// PurchaseOrderLine (04_DATABASE_GUIDELINES.md).
/// </summary>
public sealed class DispatchLineConfiguration : IEntityTypeConfiguration<DispatchLine>
{
    public void Configure(EntityTypeBuilder<DispatchLine> builder)
    {
        builder.ToTable("dispatch_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityDispatched).HasColumnType("numeric(19,4)").IsRequired();
    }
}
