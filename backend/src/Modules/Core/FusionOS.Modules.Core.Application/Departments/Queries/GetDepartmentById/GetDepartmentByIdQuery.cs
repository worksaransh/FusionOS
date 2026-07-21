using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Departments.Contracts;

namespace FusionOS.Modules.Core.Application.Departments.Queries.GetDepartmentById;

public sealed record GetDepartmentByIdQuery(Guid CompanyId, Guid DepartmentId)
    : IQuery<DepartmentDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.department.read" };
}
