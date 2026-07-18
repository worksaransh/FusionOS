using FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.BusinessIntelligence.Infrastructure.Persistence.Configurations;

public sealed class KpiSnapshotConfiguration : IEntityTypeConfiguration<KpiSnapshot>
{
    public void Configure(EntityTypeBuilder<KpiSnapshot> builder)
    {
        builder.ToTable("kpi_snapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to KpiDefinition: existence is validated in the command handler,
        // keeping this reference consistent with every other same-module reference
        // in this codebase (e.g. BudgetLine.AccountId, LeaveRequest.EmployeeId).
        builder.HasIndex(x => new { x.CompanyId, x.KpiDefinitionId, x.RecordedAt });
    }
}
