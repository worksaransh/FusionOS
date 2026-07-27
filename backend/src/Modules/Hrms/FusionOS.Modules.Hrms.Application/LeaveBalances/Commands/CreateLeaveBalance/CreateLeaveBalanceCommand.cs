using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.CreateLeaveBalance;

/// <summary>
/// Initializes an employee's leave balance for one LeaveType/Year. Reuses
/// "hrms.leave-balance.adjust" rather than a separate "*.create" permission —
/// the permission catalog only reserves .adjust and .read for this feature,
/// and initializing a balance is itself a mutation of the same shape as
/// AdjustLeaveBalance (set an entitlement), so a second, narrower permission
/// would add a distinction the catalog doesn't draw.
/// </summary>
public sealed record CreateLeaveBalanceCommand(Guid CompanyId, Guid EmployeeId, LeaveType LeaveType, int Year, decimal EntitledDays)
    : ICommand<LeaveBalanceDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.leave-balance.adjust" };
    public string EntityType => nameof(Domain.LeaveBalances.LeaveBalance);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
