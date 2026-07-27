namespace FusionOS.Modules.Crm.Application.Tasks.Contracts;

public sealed record FollowUpTaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset DueDate,
    string EntityType,
    Guid EntityId,
    Guid? AssignedToUserId,
    bool IsCompleted,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns a FollowUpTask aggregate into its DTO, shared by every handler that returns one.</summary>
public static class FollowUpTaskMapper
{
    public static FollowUpTaskDto ToDto(Domain.Tasks.FollowUpTask task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.DueDate,
        task.EntityType,
        task.EntityId,
        task.AssignedToUserId,
        task.IsCompleted,
        task.CompletedAt,
        task.CreatedAt);
}
