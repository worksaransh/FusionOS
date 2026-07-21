using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Queries.ListNonConformanceReports;

/// <summary>InspectionId is optional — omitted, this lists every NCR for the company; supplied, it scopes to the NCRs raised against one Inspection.</summary>
public sealed record ListNonConformanceReportsQuery(Guid CompanyId, Guid? InspectionId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<NonConformanceReportDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.non-conformance-report.read" };
}
