using FusionOS.Modules.Quality.Domain.Deviations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Configurations;

public sealed class DeviationRequestConfiguration : IEntityTypeConfiguration<DeviationRequest>
{
    public void Configure(EntityTypeBuilder<DeviationRequest> builder)
    {
        builder.ToTable("deviation_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.DecisionNote).HasMaxLength(2000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Inspection: InspectionId is optional and, even when supplied,
        // deliberately not existence-validated — see DeviationRequest's own class doc comment.
        builder.HasIndex(x => new { x.CompanyId, x.InspectionId });
        builder.HasIndex(x => new { x.CompanyId, x.Status });
    }
}
