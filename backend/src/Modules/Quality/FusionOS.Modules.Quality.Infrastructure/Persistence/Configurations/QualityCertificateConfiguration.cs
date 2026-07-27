using FusionOS.Modules.Quality.Domain.Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Configurations;

public sealed class QualityCertificateConfiguration : IEntityTypeConfiguration<QualityCertificate>
{
    public void Configure(EntityTypeBuilder<QualityCertificate> builder)
    {
        builder.ToTable("quality_certificates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CertificateNumber).HasMaxLength(50).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // One certificate per inspection — enforced here (not just in the command handler)
        // since idempotency is the whole reason this is persisted rather than derived; see
        // QualityCertificate's doc comment.
        builder.HasIndex(x => new { x.CompanyId, x.InspectionId }).IsUnique();
        builder.HasIndex(x => new { x.CompanyId, x.CertificateNumber }).IsUnique();
    }
}
