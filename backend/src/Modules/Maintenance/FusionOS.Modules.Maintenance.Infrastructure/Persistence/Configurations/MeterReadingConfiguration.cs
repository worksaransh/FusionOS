using FusionOS.Modules.Maintenance.Domain.MeterReadings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Maintenance.Infrastructure.Persistence.Configurations;

public sealed class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> builder)
    {
        builder.ToTable("meter_readings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReadingValue).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(50).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Asset: existence is validated in the command handler,
        // same convention as MaintenanceRequestConfiguration / MaintenanceScheduleConfiguration.
        builder.HasIndex(x => new { x.CompanyId, x.AssetId, x.ReadingDate });
    }
}
