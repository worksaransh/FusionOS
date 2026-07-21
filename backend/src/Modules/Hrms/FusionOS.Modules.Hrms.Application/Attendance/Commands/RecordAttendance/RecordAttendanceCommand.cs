using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using FusionOS.Modules.Hrms.Domain.Attendance;

namespace FusionOS.Modules.Hrms.Application.Attendance.Commands.RecordAttendance;

public sealed record RecordAttendanceCommand(
    Guid CompanyId,
    Guid EmployeeId,
    DateTimeOffset Date,
    DateTimeOffset? CheckInTime,
    DateTimeOffset? CheckOutTime,
    AttendanceStatus Status,
    Guid? LeaveRequestId)
    : ICommand<AttendanceRecordDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.attendance.record" };
    public string EntityType => nameof(Domain.Attendance.AttendanceRecord);
    public Guid EntityId { get; init; }
    public string Action => "Recorded";
}
