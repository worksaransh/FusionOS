using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Queries.GetLeaveRequestById;

public sealed record GetLeaveRequestByIdQuery(Guid CompanyId, Guid LeaveRequestId) : IQuery<LeaveRequestDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.leave-request.read" };
}
