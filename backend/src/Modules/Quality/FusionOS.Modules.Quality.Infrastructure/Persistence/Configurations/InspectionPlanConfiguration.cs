using FusionOS.Modules.Quality.Domain.InspectionPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Quality.Infrastructure.Persistence.Configurations;

public sealed class InspectionPlanConfiguration : IEntityTypeConfiguration<InspectionPlan>
{
    public void Configure(EntityTypeBuilder<InspectionPlan> builder)
    {
        builder.ToTable("inspection_plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.InspectionType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.InspectionType });
        builder.HasIndex(x => new { x.CompanyId, x.IsActive });

        builder.HasMany(x => x.Characteristics)
            .WithOne()
            .HasForeignKey("InspectionPlanId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Characteristics).HasField("_characteristics").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class InspectionPlanCharacteristicConfiguration : IEntityTypeConfiguration<InspectionPlanCharacteristic>
{
    public void Configure(EntityTypeBuilder<InspectionPlanCharacteristic> builder)
    {
        builder.ToTable("inspection_plan_characteristics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CharacteristicName).HasMaxLength(200).IsRequired();
    }
}
