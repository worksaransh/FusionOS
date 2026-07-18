using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Hrms.Domain.Employees;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Persistence;

/// <summary>
/// Owns the "hrms" schema. First real slice (2026-07-18): employee records
/// (Employee) and leave requests against them (LeaveRequest) —
/// 05_MODULE_ROADMAP.md's "Employee records" and "Leave" line items.
/// Attendance/Payroll/Recruitment/Performance/Training are not yet mapped
/// here — separately-scoped follow-ups.
/// </summary>
public sealed class HrmsDbContext : BaseDbContext
{
    public HrmsDbContext(DbContextOptions<HrmsDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("hrms");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
