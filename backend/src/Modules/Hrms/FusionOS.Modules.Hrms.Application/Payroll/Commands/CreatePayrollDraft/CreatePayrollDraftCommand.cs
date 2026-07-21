using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;

namespace FusionOS.Modules.Hrms.Application.Payroll.Commands.CreatePayrollDraft;

/// <summary>
/// Creates the one payroll record for an employee/period. GrossPay is computed
/// trivially as BaseSalary — see PayrollRecord.cs's own doc comment for why
/// nothing more (allowances, deductions, tax) is calculated in this slice.
/// </summary>
public sealed record CreatePayrollDraftCommand(Guid CompanyId, Guid EmployeeId, int PeriodMonth, int PeriodYear, decimal BaseSalary)
    : ICommand<PayrollRecordDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.payroll.create" };
    public string EntityType => nameof(Domain.Payroll.PayrollRecord);
    public Guid EntityId { get; init; }
    public string Action => "Drafted";
}
