using FusionOS.Modules.Hrms.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Hrms.Infrastructure.Persistence.Configurations;

public sealed class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.ToTable("payroll_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BaseSalary).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.GrossPay).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Employee: existence is validated in the command handler,
        // keeping this reference consistent with every other same-module reference in this
        // codebase (e.g. LeaveRequest.EmployeeId, AttendanceRecord.EmployeeId).
        // One payroll record per employee per period.
        builder.HasIndex(x => new { x.CompanyId, x.EmployeeId, x.PeriodYear, x.PeriodMonth }).IsUnique();
        builder.HasIndex(x => new { x.CompanyId, x.PeriodYear, x.PeriodMonth });
    }
}
