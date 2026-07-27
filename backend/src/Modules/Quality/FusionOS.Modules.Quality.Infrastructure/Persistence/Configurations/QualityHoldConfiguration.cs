using FusionOS.Modules.Quality.Domain.QualityHolds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Configurations;

public sealed class QualityHoldConfiguration : IEntityTypeConfiguration<QualityHold>
{
    public void Configure(EntityTypeBuilder<QualityHold> builder)
    {
        builder.ToTable("quality_holds");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ReleasedByNote).HasMaxLength(1000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Inspection, and none to whatever (EntityType, EntityId) points
        // at either — both are opaque references, same convention as Inspection.ReferenceId
        // and Core's ApprovalRequest (EntityType, EntityId).
        builder.HasIndex(x => new { x.CompanyId, x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.CompanyId, x.Status });
        builder.HasIndex(x => x.InspectionId);
    }
}
