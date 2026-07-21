using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Queries.GetNonConformanceReportById;

public sealed record GetNonConformanceReportByIdQuery(Guid CompanyId, Guid NonConformanceReportId) : IQuery<NonConformanceReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.non-conformance-report.read" };
}
