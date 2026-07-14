using FusionOS.Modules.Finance.Domain.JournalEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reference).HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.TotalDebit);
        builder.Ignore(x => x.TotalCredit);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.EntryDate });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("JournalEntryId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
