using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.Reports.Contracts;

namespace FusionOS.Modules.Maintenance.Application.Reports.Queries.GetDowntimeMetricsReport;

/// <summary>AssetId is optional — omitted, this reports every asset with at least one completed request in the period; supplied, it scopes to that one asset, same optional-scope convention as ListMaintenanceRequestsQuery.AssetId.</summary>
public sealed record GetDowntimeMetricsReportQuery(Guid CompanyId, Guid? AssetId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd)
    : IQuery<DowntimeMetricsReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.downtime-report.read" };
}
