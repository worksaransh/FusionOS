using FusionOS.Modules.Core.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(200).IsRequired();

        // The storage decision (bytea, not a filesystem/S3/Azure Blob integration
        // that has no configured backend to point at) — see Document's own doc
        // comment for the full reasoning and the honest scaling limitation.
        builder.Property(x => x.Content).HasColumnType("bytea").IsRequired();

        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        // Backs both the AttachmentsPanel "list files for this record" query and
        // DocumentRepository.CountByEntityAsync — same shape as
        // ApprovalRequestConfiguration's (CompanyId, EntityType, EntityId) index.
        builder.HasIndex(x => new { x.CompanyId, x.EntityType, x.EntityId });
    }
}
