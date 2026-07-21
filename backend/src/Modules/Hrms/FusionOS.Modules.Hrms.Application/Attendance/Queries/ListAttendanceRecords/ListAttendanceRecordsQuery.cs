using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;

namespace FusionOS.Modules.Hrms.Application.Attendance.Queries.ListAttendanceRecords;

/// <summary>EmployeeId/StartDate/EndDate are all optional filters — omitted, this lists every attendance record for the company; supplied, it scopes to one employee and/or a date range, same optional-filter shape as ListLeaveRequestsQuery's EmployeeId.</summary>
public sealed record ListAttendanceRecordsQuery(Guid CompanyId, Guid? EmployeeId = null, DateTimeOffset? StartDate = null, DateTimeOffset? EndDate = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<AttendanceRecordDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.attendance.read" };
}
