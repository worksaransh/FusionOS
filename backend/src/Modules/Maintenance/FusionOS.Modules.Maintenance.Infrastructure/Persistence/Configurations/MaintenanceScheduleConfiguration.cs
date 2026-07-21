using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Maintenance.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceScheduleConfiguration : IEntityTypeConfiguration<MaintenanceSchedule>
{
    public void Configure(EntityTypeBuilder<MaintenanceSchedule> builder)
    {
        builder.ToTable("maintenance_schedules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Frequency).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Asset: existence is validated in the command handler,
        // same convention as MaintenanceRequest's own AssetId reference.
        builder.HasIndex(x => new { x.CompanyId, x.AssetId });
        builder.HasIndex(x => new { x.CompanyId, x.NextDueDate });
    }
}
