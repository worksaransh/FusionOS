using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;

namespace FusionOS.Modules.Hrms.Application.Attendance.Queries.GetAttendanceRecordById;

public sealed record GetAttendanceRecordByIdQuery(Guid CompanyId, Guid AttendanceRecordId) : IQuery<AttendanceRecordDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.attendance.read" };
}
