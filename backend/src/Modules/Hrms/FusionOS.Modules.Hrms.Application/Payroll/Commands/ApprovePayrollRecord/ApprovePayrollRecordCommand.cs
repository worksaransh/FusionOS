using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;

namespace FusionOS.Modules.Hrms.Application.Payroll.Commands.ApprovePayrollRecord;

public sealed record ApprovePayrollRecordCommand(Guid CompanyId, Guid PayrollRecordId)
    : ICommand<PayrollRecordDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.payroll.approve" };
    public string EntityType => nameof(Domain.Payroll.PayrollRecord);
    public Guid EntityId => PayrollRecordId;
    public string Action => "Approved";
}
