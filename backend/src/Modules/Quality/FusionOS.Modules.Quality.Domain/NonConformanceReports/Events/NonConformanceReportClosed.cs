using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.NonConformanceReports.Events;

/// <summary>
/// Raised when an NCR's status is moved to Closed. No consumer today — the natural future
/// hook is closing out any CorrectiveAction plans still open against this NCR, deliberately
/// out of scope for this slice (CorrectiveAction's own lifecycle is independent).
/// </summary>
public sealed record NonConformanceReportClosed(Guid NonConformanceReportId, Guid CompanyId, Guid? InspectionId, string Severity) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
