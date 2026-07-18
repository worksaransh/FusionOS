using FusionOS.Modules.Finance.Domain.JournalEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// Was previously missing entirely — JournalEntryLine's Debit/Credit columns
/// fell back to EF Core's default decimal mapping instead of the numeric(19,4)
/// every sibling line entity uses (PurchaseOrderLineConfiguration,
/// SalesOrderLineConfiguration), a real precision inconsistency across the
/// ledger that the enterprise audit flagged (04_DATABASE_GUIDELINES.md §4).
/// See JournalEntryLine's doc comment for why this table has no audit/tenant
/// columns of its own — same reasoning as the other line entities.
/// </summary>
public sealed class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("journal_entry_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Debit).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Credit).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        // Optional same-module cost-center reference (nullable). Indexed to support
        // the cost-center-filtered Budget vs-actual sum without a full table scan.
        // No FK constraint declared: existence is validated in the command handler,
        // keeping the reference consistent with how AccountId is treated on this line.
        builder.Property(x => x.CostCenterId);
        builder.HasIndex(x => x.CostCenterId);
    }
}
