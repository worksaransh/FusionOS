using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.AdjustLeaveBalance;

/// <summary>Manual correction/accrual top-up of an existing balance's entitlement — see LeaveBalance.AdjustEntitlement.</summary>
public sealed record AdjustLeaveBalanceCommand(Guid CompanyId, Guid LeaveBalanceId, decimal NewEntitledDays)
    : ICommand<LeaveBalanceDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.leave-balance.adjust" };
    public string EntityType => nameof(Domain.LeaveBalances.LeaveBalance);
    public Guid EntityId => LeaveBalanceId;
    public string Action => "Adjusted";
}
