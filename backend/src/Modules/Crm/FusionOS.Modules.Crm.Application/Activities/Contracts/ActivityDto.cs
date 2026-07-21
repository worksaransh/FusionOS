namespace FusionOS.Modules.Crm.Application.Activities.Contracts;

public sealed record ActivityDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Type,
    string Notes,
    DateTimeOffset CreatedAt,
    Guid CreatedBy);

/// <summary>Single place that turns an Activity aggregate into its DTO, shared by every handler that returns one.</summary>
public static class ActivityMapper
{
    public static ActivityDto ToDto(Domain.Activities.Activity activity) => new(
        activity.Id,
        activity.EntityType,
        activity.EntityId,
        activity.Type.ToString(),
        activity.Notes,
        activity.CreatedAt,
        activity.CreatedBy);
}
