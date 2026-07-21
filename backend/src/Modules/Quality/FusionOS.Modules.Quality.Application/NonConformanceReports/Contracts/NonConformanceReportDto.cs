namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;

public sealed record NonConformanceReportDto(
    Guid Id,
    Guid? InspectionId,
    string Description,
    string Severity,
    string Status,
    Guid RaisedByUserId,
    DateTimeOffset RaisedAt,
    DateTimeOffset? ClosedAt);

/// <summary>Single place that turns a NonConformanceReport aggregate into its DTO, shared by every handler that returns one.</summary>
public static class NonConformanceReportMapper
{
    public static NonConformanceReportDto ToDto(Domain.NonConformanceReports.NonConformanceReport report) => new(
        report.Id,
        report.InspectionId,
        report.Description,
        report.Severity.ToString(),
        report.Status.ToString(),
        report.RaisedByUserId,
        report.RaisedAt,
        report.ClosedAt);
}
