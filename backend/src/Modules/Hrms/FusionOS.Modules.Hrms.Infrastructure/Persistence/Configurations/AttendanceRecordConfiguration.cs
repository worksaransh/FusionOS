using FusionOS.Modules.Hrms.Domain.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Hrms.Infrastructure.Persistence.Configurations;

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Employee or LeaveRequest: existence is validated in the command
        // handler, keeping both references consistent with every other same-module reference
        // in this codebase (e.g. LeaveRequest.EmployeeId, BudgetLine.AccountId).
        // One attendance record per employee per calendar date.
        builder.HasIndex(x => new { x.CompanyId, x.EmployeeId, x.Date }).IsUnique();
        builder.HasIndex(x => new { x.CompanyId, x.Date });
    }
}
