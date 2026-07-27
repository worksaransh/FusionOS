using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tasks.Contracts;

namespace FusionOS.Modules.Crm.Application.Tasks.Commands.CompleteFollowUpTask;

public sealed record CompleteFollowUpTaskCommand(Guid CompanyId, Guid TaskId)
    : ICommand<FollowUpTaskDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.task.complete" };
    public string EntityType => nameof(Domain.Tasks.FollowUpTask);
    public Guid EntityId => TaskId;
    public string Action => "Completed";
}
