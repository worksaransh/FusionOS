using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.NonConformanceReports.Events;

/// <summary>Raised when an NCR is opened. Kept for consistency; no consumer today.</summary>
public sealed record NonConformanceReportCreated(Guid NonConformanceReportId, Guid CompanyId, Guid? InspectionId, string Severity) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
