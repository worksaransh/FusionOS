using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Queries.ListMaintenanceRequests;

/// <summary>AssetId is optional — omitted, this lists every request for the company; supplied, it scopes to one Asset's maintenance history (05_MODULE_ROADMAP.md's "Maintenance history" line item).</summary>
public sealed record ListMaintenanceRequestsQuery(Guid CompanyId, Guid? AssetId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<MaintenanceRequestDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.request.read" };
}
