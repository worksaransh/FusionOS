namespace FusionOS.Modules.Hrms.Application.Attendance.Contracts;

public sealed record AttendanceRecordDto(
    Guid Id,
    Guid EmployeeId,
    DateTimeOffset Date,
    DateTimeOffset? CheckInTime,
    DateTimeOffset? CheckOutTime,
    string Status,
    Guid? LeaveRequestId);

/// <summary>Single place that turns an AttendanceRecord aggregate into its DTO, shared by every handler that returns one.</summary>
public static class AttendanceRecordMapper
{
    public static AttendanceRecordDto ToDto(Domain.Attendance.AttendanceRecord record) => new(
        record.Id,
        record.EmployeeId,
        record.Date,
        record.CheckInTime,
        record.CheckOutTime,
        record.Status.ToString(),
        record.LeaveRequestId);
}
