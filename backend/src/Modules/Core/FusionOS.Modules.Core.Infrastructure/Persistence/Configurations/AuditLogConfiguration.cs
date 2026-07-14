using FusionOS.Modules.Core.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.HasIndex(a => new { a.CompanyId, a.EntityType, a.EntityId });
        // Insert-only per 04_DATABASE_GUIDELINES.md §5 — no update/delete path is ever exposed for this table.
    }
}
