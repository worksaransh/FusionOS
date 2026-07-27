using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Queries.GetLeaveBalance;

public sealed record GetLeaveBalanceQuery(Guid CompanyId, Guid EmployeeId, LeaveType LeaveType, int Year)
    : IQuery<LeaveBalanceDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.leave-balance.read" };
}
