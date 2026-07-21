namespace FusionOS.Modules.Hrms.Domain.Attendance;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum (04_DATABASE_GUIDELINES.md §10) — same convention as LeaveType/LeaveRequestStatus.</summary>
public enum AttendanceStatus
{
    Present,
    Absent,
    HalfDay,
    OnLeave,
}
