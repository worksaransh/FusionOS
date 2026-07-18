namespace FusionOS.Modules.Ai.Application.Recommendations.Contracts;

public sealed record RecommendationDto(
    Guid Id,
    string Type,
    Guid ReferenceId,
    string Summary,
    string ModelVersion,
    string Status,
    DateTimeOffset? DecidedAt);

/// <summary>Single place that turns a Recommendation aggregate into its DTO, shared by every handler that returns one.</summary>
public static class RecommendationMapper
{
    public static RecommendationDto ToDto(Domain.Recommendations.Recommendation recommendation) => new(
        recommendation.Id,
        recommendation.Type,
        recommendation.ReferenceId,
        recommendation.Summary,
        recommendation.ModelVersion,
        recommendation.Status.ToString(),
        recommendation.DecidedAt);
}
