using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using FusionOS.Modules.Quality.Domain.NonConformanceReports;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.CreateNonConformanceReport;

public sealed record CreateNonConformanceReportCommand(
    Guid CompanyId,
    Guid? InspectionId,
    string Description,
    NonConformanceReportSeverity Severity,
    Guid RaisedByUserId)
    : ICommand<NonConformanceReportDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.non-conformance-report.create" };
    public string EntityType => nameof(Domain.NonConformanceReports.NonConformanceReport);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
