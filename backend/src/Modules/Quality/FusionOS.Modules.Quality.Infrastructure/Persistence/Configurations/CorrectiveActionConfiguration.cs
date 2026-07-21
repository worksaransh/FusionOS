using FusionOS.Modules.Quality.Domain.CorrectiveActions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Configurations;

public sealed class CorrectiveActionConfiguration : IEntityTypeConfiguration<CorrectiveAction>
{
    public void Configure(EntityTypeBuilder<CorrectiveAction> builder)
    {
        builder.ToTable("corrective_actions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RootCauseDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.CorrectiveActionDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.PreventiveActionDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to NonConformanceReport: existence is validated in the command
        // handler, keeping this reference consistent with every other same-module reference
        // in this codebase (e.g. MaintenanceRequest.AssetId). AssignedToUserId is an opaque
        // cross-module reference into Core's User, never existence-validated at all.
        builder.HasIndex(x => new { x.CompanyId, x.NonConformanceReportId });
        builder.HasIndex(x => new { x.CompanyId, x.Status });
        builder.HasIndex(x => new { x.CompanyId, x.AssignedToUserId });
    }
}
