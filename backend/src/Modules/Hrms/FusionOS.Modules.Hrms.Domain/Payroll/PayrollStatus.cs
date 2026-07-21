namespace FusionOS.Modules.Hrms.Domain.Payroll;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum (04_DATABASE_GUIDELINES.md §10) — same convention as LeaveType/LeaveRequestStatus/AttendanceStatus.</summary>
public enum PayrollStatus
{
    Draft,
    Approved,
    Paid,
}
