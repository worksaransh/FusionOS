using FusionOS.Modules.Quality.Domain.NonConformanceReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Configurations;

public sealed class NonConformanceReportConfiguration : IEntityTypeConfiguration<NonConformanceReport>
{
    public void Configure(EntityTypeBuilder<NonConformanceReport> builder)
    {
        builder.ToTable("non_conformance_reports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Inspection: existence is validated in the command handler when
        // supplied, keeping this reference consistent with every other same-module reference
        // in this codebase (e.g. MaintenanceRequest.AssetId).
        builder.HasIndex(x => new { x.CompanyId, x.InspectionId });
        builder.HasIndex(x => new { x.CompanyId, x.Status });
    }
}
