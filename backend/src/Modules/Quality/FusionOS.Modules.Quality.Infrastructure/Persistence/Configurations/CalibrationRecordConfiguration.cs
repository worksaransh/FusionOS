using FusionOS.Modules.Quality.Domain.Calibrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Configurations;

public sealed class CalibrationRecordConfiguration : IEntityTypeConfiguration<CalibrationRecord>
{
    public void Configure(EntityTypeBuilder<CalibrationRecord> builder)
    {
        builder.ToTable("calibration_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EquipmentName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EquipmentIdentifier).HasMaxLength(100);
        builder.Property(x => x.CalibrationStandard).HasMaxLength(100);
        builder.Property(x => x.Result).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.PerformedByExternalVendor).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.EquipmentReferenceId });
        // Backs the overdue filter (NextCalibrationDueDate < now) ListCalibrationRecordsQuery uses.
        builder.HasIndex(x => new { x.CompanyId, x.NextCalibrationDueDate });
    }
}
