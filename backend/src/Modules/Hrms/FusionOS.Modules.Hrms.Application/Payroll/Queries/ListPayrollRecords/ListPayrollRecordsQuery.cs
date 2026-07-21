using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;

namespace FusionOS.Modules.Hrms.Application.Payroll.Queries.ListPayrollRecords;

/// <summary>EmployeeId/PeriodMonth/PeriodYear are all optional filters — omitted, this lists every payroll record for the company; supplied, it scopes down, same optional-filter shape as ListLeaveRequestsQuery's EmployeeId.</summary>
public sealed record ListPayrollRecordsQuery(Guid CompanyId, Guid? EmployeeId = null, int? PeriodMonth = null, int? PeriodYear = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<PayrollRecordDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.payroll.read" };
}
