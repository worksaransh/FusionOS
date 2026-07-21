namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;

public sealed record CorrectiveActionDto(
    Guid Id,
    Guid NonConformanceReportId,
    string RootCauseDescription,
    string CorrectiveActionDescription,
    string PreventiveActionDescription,
    Guid AssignedToUserId,
    DateTimeOffset DueDate,
    string Status,
    DateTimeOffset? ClosedAt,
    DateTimeOffset? VerifiedAt);

/// <summary>Single place that turns a CorrectiveAction aggregate into its DTO, shared by every handler that returns one.</summary>
public static class CorrectiveActionMapper
{
    public static CorrectiveActionDto ToDto(Domain.CorrectiveActions.CorrectiveAction correctiveAction) => new(
        correctiveAction.Id,
        correctiveAction.NonConformanceReportId,
        correctiveAction.RootCauseDescription,
        correctiveAction.CorrectiveActionDescription,
        correctiveAction.PreventiveActionDescription,
        correctiveAction.AssignedToUserId,
        correctiveAction.DueDate,
        correctiveAction.Status.ToString(),
        correctiveAction.ClosedAt,
        correctiveAction.VerifiedAt);
}
