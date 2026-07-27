using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tasks.Contracts;

namespace FusionOS.Modules.Crm.Application.Tasks.Commands.CreateFollowUpTask;

/// <summary>
/// Creates a follow-up reminder against some (EntityType, EntityId) pair — same
/// EntityType/EntityId-doubles-as-the-IAuditableCommand-identity convention as
/// CreateActivityCommand: these constructor parameters satisfy IAuditableCommand
/// directly, no separate declaration needed.
/// </summary>
public sealed record CreateFollowUpTaskCommand(
    Guid CompanyId,
    string Title,
    string? Description,
    DateTimeOffset DueDate,
    string EntityType,
    Guid EntityId,
    Guid? AssignedToUserId)
    : ICommand<FollowUpTaskDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.task.create" };
    public string Action => "Created";
}
