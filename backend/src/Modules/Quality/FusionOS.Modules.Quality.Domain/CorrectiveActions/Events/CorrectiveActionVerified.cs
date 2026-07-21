using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.CorrectiveActions.Events;

/// <summary>Raised when a closed CAPA plan is verified effective. No consumer today — the natural future hook is closing out the parent NonConformanceReport once every CAPA against it is verified, deliberately out of scope for this slice.</summary>
public sealed record CorrectiveActionVerified(Guid CorrectiveActionId, Guid CompanyId, Guid NonConformanceReportId, Guid AssignedToUserId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
