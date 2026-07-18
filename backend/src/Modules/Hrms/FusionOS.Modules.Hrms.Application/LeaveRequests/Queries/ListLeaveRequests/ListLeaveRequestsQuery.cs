using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Queries.ListLeaveRequests;

/// <summary>EmployeeId is optional — omitted, this lists every leave request for the company; supplied, it scopes to one employee's leave history.</summary>
public sealed record ListLeaveRequestsQuery(Guid CompanyId, Guid? EmployeeId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<LeaveRequestDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.leave-request.read" };
}
