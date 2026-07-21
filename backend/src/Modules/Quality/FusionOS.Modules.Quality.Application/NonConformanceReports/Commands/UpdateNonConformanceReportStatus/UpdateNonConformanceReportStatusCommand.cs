using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using FusionOS.Modules.Quality.Domain.NonConformanceReports;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.UpdateNonConformanceReportStatus;

public sealed record UpdateNonConformanceReportStatusCommand(Guid CompanyId, Guid NonConformanceReportId, NonConformanceReportStatus Status)
    : ICommand<NonConformanceReportDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.non-conformance-report.update-status" };
    public string EntityType => nameof(Domain.NonConformanceReports.NonConformanceReport);
    public Guid EntityId => NonConformanceReportId;
    public string Action => "StatusUpdated";
}
