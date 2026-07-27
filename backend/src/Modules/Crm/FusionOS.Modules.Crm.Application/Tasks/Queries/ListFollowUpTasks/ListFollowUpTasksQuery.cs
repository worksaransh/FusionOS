using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tasks.Contracts;

namespace FusionOS.Modules.Crm.Application.Tasks.Queries.ListFollowUpTasks;

/// <summary>
/// AssignedToUserId and (EntityType, EntityId) are optional and independent (omit both
/// for a company-wide feed); IsCompleted filters done vs open; OverdueOnly further
/// restricts to open tasks whose DueDate has already passed "now" — see
/// IFollowUpTaskRepository for the exact filter semantics.
/// </summary>
public sealed record ListFollowUpTasksQuery(
    Guid CompanyId,
    Guid? AssignedToUserId = null,
    string? EntityType = null,
    Guid? EntityId = null,
    bool? IsCompleted = null,
    bool OverdueOnly = false,
    int Page = 1,
    int PageSize = 25)
    : IQuery<PagedResult<FollowUpTaskDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.task.read" };
}
