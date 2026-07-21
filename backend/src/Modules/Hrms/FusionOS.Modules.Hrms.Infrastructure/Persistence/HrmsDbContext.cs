using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Hrms.Domain.Attendance;
using FusionOS.Modules.Hrms.Domain.Employees;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using FusionOS.Modules.Hrms.Domain.Payroll;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Persistence;

/// <summary>
/// Owns the "hrms" schema. First real slice (2026-07-18): employee records
/// (Employee) and leave requests against them (LeaveRequest) —
/// 05_MODULE_ROADMAP.md's "Employee records" and "Leave" line items.
/// Second slice (2026-07-20): attendance (AttendanceRecord) and a
/// deliberately minimal payroll skeleton (PayrollRecord) —
/// 05_MODULE_ROADMAP.md's "Attendance" and "Payroll" line items (see
/// PayrollRecord.cs's own doc comment for exactly how minimal). Recruitment/
/// Performance/Training remain out of scope — separately-scoped follow-ups.
/// </summary>
public sealed class HrmsDbContext : BaseDbContext
{
    public HrmsDbContext(DbContextOptions<HrmsDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("hrms");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
