using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using FusionOS.Modules.Hrms.Domain.Attendance;

namespace FusionOS.Modules.Hrms.Application.Attendance.Commands.UpdateAttendance;

public sealed record UpdateAttendanceCommand(
    Guid CompanyId,
    Guid AttendanceRecordId,
    DateTimeOffset? CheckInTime,
    DateTimeOffset? CheckOutTime,
    AttendanceStatus Status,
    Guid? LeaveRequestId)
    : ICommand<AttendanceRecordDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.attendance.update" };
    public string EntityType => nameof(Domain.Attendance.AttendanceRecord);
    public Guid EntityId => AttendanceRecordId;
    public string Action => "Updated";
}
