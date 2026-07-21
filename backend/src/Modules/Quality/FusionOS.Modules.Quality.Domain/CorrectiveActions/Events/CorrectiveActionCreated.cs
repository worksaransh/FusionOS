using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.CorrectiveActions.Events;

/// <summary>Raised when a CAPA plan is opened against a non-conformance report. Kept for consistency; no consumer today.</summary>
public sealed record CorrectiveActionCreated(Guid CorrectiveActionId, Guid CompanyId, Guid NonConformanceReportId, Guid AssignedToUserId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
