using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Departments.Contracts;

namespace FusionOS.Modules.Core.Application.Departments.Queries.ListDepartments;

public sealed record ListDepartmentsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<DepartmentDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.department.read" };
}
