using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;

namespace FusionOS.Modules.Hrms.Application.Payroll.Queries.GetPayrollRecordById;

public sealed record GetPayrollRecordByIdQuery(Guid CompanyId, Guid PayrollRecordId) : IQuery<PayrollRecordDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.payroll.read" };
}
