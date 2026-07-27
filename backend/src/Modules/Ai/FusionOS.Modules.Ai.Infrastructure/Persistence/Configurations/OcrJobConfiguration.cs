using FusionOS.Modules.Ai.Domain.OcrJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Ai.Infrastructure.Persistence.Configurations;

public sealed class OcrJobConfiguration : IEntityTypeConfiguration<OcrJob>
{
    public void Configure(EntityTypeBuilder<OcrJob> builder)
    {
        builder.ToTable("ocr_jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ExtractedFieldsJson).HasColumnType("text");
        builder.Property(x => x.FailureReason).HasMaxLength(2000);
        builder.Property(x => x.DraftReferenceType).HasMaxLength(100);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.Status });
        builder.HasIndex(x => x.DocumentId);
    }
}
