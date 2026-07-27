namespace FusionOS.Modules.Quality.Application.QualityHolds.Contracts;

public sealed record QualityHoldDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Reason,
    Guid? InspectionId,
    string Status,
    DateTimeOffset? ReleasedAt,
    string? ReleasedByNote);

/// <summary>Single place that turns a QualityHold aggregate into its DTO, shared by every handler that returns one.</summary>
public static class QualityHoldMapper
{
    public static QualityHoldDto ToDto(Domain.QualityHolds.QualityHold hold) => new(
        hold.Id,
        hold.EntityType,
        hold.EntityId,
        hold.Reason,
        hold.InspectionId,
        hold.Status.ToString(),
        hold.ReleasedAt,
        hold.ReleasedByNote);
}
