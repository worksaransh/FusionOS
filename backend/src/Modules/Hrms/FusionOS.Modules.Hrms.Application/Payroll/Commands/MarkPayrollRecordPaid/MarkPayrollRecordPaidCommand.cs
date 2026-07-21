using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;

namespace FusionOS.Modules.Hrms.Application.Payroll.Commands.MarkPayrollRecordPaid;

public sealed record MarkPayrollRecordPaidCommand(Guid CompanyId, Guid PayrollRecordId)
    : ICommand<PayrollRecordDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.payroll.mark-paid" };
    public string EntityType => nameof(Domain.Payroll.PayrollRecord);
    public Guid EntityId => PayrollRecordId;
    public string Action => "MarkedPaid";
}
