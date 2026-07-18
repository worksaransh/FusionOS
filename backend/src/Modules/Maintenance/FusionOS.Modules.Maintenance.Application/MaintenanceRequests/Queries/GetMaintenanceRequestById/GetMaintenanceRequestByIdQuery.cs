using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Queries.GetMaintenanceRequestById;

public sealed record GetMaintenanceRequestByIdQuery(Guid CompanyId, Guid MaintenanceRequestId) : IQuery<MaintenanceRequestDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.request.read" };
}
