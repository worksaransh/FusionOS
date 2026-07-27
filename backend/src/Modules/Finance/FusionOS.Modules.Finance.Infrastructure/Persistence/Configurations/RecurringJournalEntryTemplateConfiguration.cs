using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class RecurringJournalEntryTemplateConfiguration : IEntityTypeConfiguration<RecurringJournalEntryTemplate>
{
    public void Configure(EntityTypeBuilder<RecurringJournalEntryTemplate> builder)
    {
        builder.ToTable("recurring_journal_entry_templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reference).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Frequency).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.TotalDebit);
        builder.Ignore(x => x.TotalCredit);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.NextDueDate });
        builder.HasIndex(x => new { x.CompanyId, x.IsActive });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("RecurringJournalEntryTemplateId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
