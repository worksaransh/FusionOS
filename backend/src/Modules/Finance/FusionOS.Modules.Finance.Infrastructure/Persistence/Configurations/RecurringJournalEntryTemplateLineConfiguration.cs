using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

/// <summary>Mirrors JournalEntryLineConfiguration's shape — same numeric(19,4) precision for Debit/Credit, same "no FK constraint, existence validated by the command handler" treatment of CostCenterId.</summary>
public sealed class RecurringJournalEntryTemplateLineConfiguration : IEntityTypeConfiguration<RecurringJournalEntryTemplateLine>
{
    public void Configure(EntityTypeBuilder<RecurringJournalEntryTemplateLine> builder)
    {
        builder.ToTable("recurring_journal_entry_template_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Debit).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Credit).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.CostCenterId);
        builder.HasIndex(x => x.CostCenterId);
    }
}
