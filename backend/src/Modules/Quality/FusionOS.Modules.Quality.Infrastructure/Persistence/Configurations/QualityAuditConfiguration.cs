using FusionOS.Modules.Quality.Domain.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Configurations;

public sealed class QualityAuditConfiguration : IEntityTypeConfiguration<QualityAudit>
{
    public void Configure(EntityTypeBuilder<QualityAudit> builder)
    {
        builder.ToTable("quality_audits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AuditType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Auditor).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Scope).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.Status });

        builder.HasMany(x => x.Findings)
            .WithOne()
            .HasForeignKey("QualityAuditId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Findings).HasField("_findings").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class AuditFindingConfiguration : IEntityTypeConfiguration<AuditFinding>
{
    public void Configure(EntityTypeBuilder<AuditFinding> builder)
    {
        builder.ToTable("audit_findings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
