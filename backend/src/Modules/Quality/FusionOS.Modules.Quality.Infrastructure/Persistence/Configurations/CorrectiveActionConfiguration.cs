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
        // Structured RCA (additive, 2026-07-23): RcaMethod defaults to "Other" so every
        // pre-existing row keeps working unchanged; the 5-Whys chain is 5 plain nullable
        // columns rather than a child entity — see CorrectiveAction's own doc comment.
        builder.Property(x => x.RcaMethod).HasConversion<string>().HasMaxLength(20).IsRequired().HasDefaultValue(RcaMethod.Other);
        builder.Property(x => x.Why1).HasMaxLength(500);
        builder.Property(x => x.Why2).HasMaxLength(500);
        builder.Property(x => x.Why3).HasMaxLength(500);
        builder.Property(x => x.Why4).HasMaxLength(500);
        builder.Property(x => x.Why5).HasMaxLength(500);
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
