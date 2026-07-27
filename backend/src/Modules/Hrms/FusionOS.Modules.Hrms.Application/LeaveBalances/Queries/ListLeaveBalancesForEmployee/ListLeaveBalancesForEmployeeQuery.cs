using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Queries.ListLeaveBalancesForEmployee;

/// <summary>All of one employee's leave balances (every type/year on record) — bespoke array, not a PagedResult, same convention as ListMachinesByWorkCenterQuery for a small, naturally-bounded roster.</summary>
public sealed record ListLeaveBalancesForEmployeeQuery(Guid CompanyId, Guid EmployeeId)
    : IQuery<IReadOnlyList<LeaveBalanceDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.leave-balance.read" };
}
