namespace FusionOS.Modules.Hrms.Application.Payroll.Contracts;

public sealed record PayrollRecordDto(
    Guid Id,
    Guid EmployeeId,
    int PeriodMonth,
    int PeriodYear,
    decimal BaseSalary,
    decimal GrossPay,
    string Status,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? PaidAt);

/// <summary>Single place that turns a PayrollRecord aggregate into its DTO, shared by every handler that returns one.</summary>
public static class PayrollRecordMapper
{
    public static PayrollRecordDto ToDto(Domain.Payroll.PayrollRecord record) => new(
        record.Id,
        record.EmployeeId,
        record.PeriodMonth,
        record.PeriodYear,
        record.BaseSalary,
        record.GrossPay,
        record.Status.ToString(),
        record.ApprovedAt,
        record.PaidAt);
}
