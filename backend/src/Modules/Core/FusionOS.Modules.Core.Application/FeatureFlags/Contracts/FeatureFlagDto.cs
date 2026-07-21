namespace FusionOS.Modules.Core.Application.FeatureFlags.Contracts;

public sealed record FeatureFlagDto(
    Guid Id,
    string Key,
    string Name,
    string? Description,
    bool IsEnabled,
    int RolloutPercentage,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns a FeatureFlag aggregate into its DTO, shared by every handler that returns one — same convention as NonConformanceReportMapper/CreateCostCenterCommandHandler.MapToDto.</summary>
public static class FeatureFlagMapper
{
    public static FeatureFlagDto ToDto(Domain.FeatureFlags.FeatureFlag flag) => new(
        flag.Id,
        flag.Key,
        flag.Name,
        flag.Description,
        flag.IsEnabled,
        flag.RolloutPercentage,
        flag.CreatedAt);
}
