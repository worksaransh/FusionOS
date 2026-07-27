using FusionOS.Modules.Ai.Domain.DemandHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Ai.Infrastructure.Persistence.Configurations;

public sealed class DemandHistoryEntryConfiguration : IEntityTypeConfiguration<DemandHistoryEntry>
{
    public void Configure(EntityTypeBuilder<DemandHistoryEntry> builder)
    {
        builder.ToTable("demand_history_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityDispatched).HasColumnType("numeric(18,4)").IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.PeriodDate });
    }
}
