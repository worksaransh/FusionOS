namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;

public sealed record LeaveBalanceDto(
    Guid Id,
    Guid EmployeeId,
    string LeaveType,
    int Year,
    decimal EntitledDays,
    decimal UsedDays,
    decimal RemainingDays);

/// <summary>Single place that turns a LeaveBalance aggregate into its DTO, shared by every handler that returns one.</summary>
public static class LeaveBalanceMapper
{
    public static LeaveBalanceDto ToDto(Domain.LeaveBalances.LeaveBalance balance) => new(
        balance.Id,
        balance.EmployeeId,
        balance.LeaveType.ToString(),
        balance.Year,
        balance.EntitledDays,
        balance.UsedDays,
        balance.RemainingDays);
}
