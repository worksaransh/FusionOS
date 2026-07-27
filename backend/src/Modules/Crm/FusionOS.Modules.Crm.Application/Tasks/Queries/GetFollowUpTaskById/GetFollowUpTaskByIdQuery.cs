using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tasks.Contracts;

namespace FusionOS.Modules.Crm.Application.Tasks.Queries.GetFollowUpTaskById;

public sealed record GetFollowUpTaskByIdQuery(Guid CompanyId, Guid TaskId) : IQuery<FollowUpTaskDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.task.read" };
}
