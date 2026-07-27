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
    DateTimeOffset? VerifiedAt,
    string RcaMethod,
    string? Why1,
    string? Why2,
    string? Why3,
    string? Why4,
    string? Why5);

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
        correctiveAction.VerifiedAt,
        correctiveAction.RcaMethod.ToString(),
        correctiveAction.Why1,
        correctiveAction.Why2,
        correctiveAction.Why3,
        correctiveAction.Why4,
        correctiveAction.Why5);
}
