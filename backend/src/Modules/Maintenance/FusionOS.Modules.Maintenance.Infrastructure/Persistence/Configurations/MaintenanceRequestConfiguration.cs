using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Maintenance.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.ToTable("maintenance_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ResolutionNotes).HasMaxLength(1000);
        builder.Property(x => x.LaborHours).HasColumnType("numeric(19,4)");
        builder.Property(x => x.LaborRatePerHour).HasColumnType("numeric(19,4)");
        builder.Ignore(x => x.TotalCost); // Computed on read from LaborHours/LaborRatePerHour + SparePartUsages — never persisted.
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Asset: existence is validated in the command handler,
        // keeping this reference consistent with every other same-module reference
        // in this codebase (e.g. BudgetLine.AccountId).
        builder.HasIndex(x => new { x.CompanyId, x.AssetId });
        builder.HasIndex(x => new { x.CompanyId, x.Status });

        builder.HasMany(x => x.SparePartUsages)
            .WithOne()
            .HasForeignKey("MaintenanceRequestId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.SparePartUsages).HasField("_sparePartUsages").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.ChecklistItems)
            .WithOne()
            .HasForeignKey("MaintenanceRequestId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.ChecklistItems).HasField("_checklistItems").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class SparePartUsageConfiguration : IEntityTypeConfiguration<SparePartUsage>
{
    public void Configure(EntityTypeBuilder<SparePartUsage> builder)
    {
        builder.ToTable("maintenance_request_spare_part_usages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityUsed).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("numeric(19,4)").IsRequired();
        builder.HasIndex(x => x.ProductId);
    }
}

public sealed class MaintenanceRequestChecklistItemConfiguration : IEntityTypeConfiguration<MaintenanceRequestChecklistItem>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequestChecklistItem> builder)
    {
        builder.ToTable("maintenance_request_checklist_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IsCompleted).IsRequired();
    }
}
