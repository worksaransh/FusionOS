using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Crm.Application.Tags.Commands.DetachTagFromEntity;

/// <summary>Removes a previously-attached Tag from a (TargetEntityType, TargetEntityId) pair — a hard delete of the join row, same as removing a row from any join table.</summary>
public sealed record DetachTagFromEntityCommand(Guid CompanyId, Guid TagId, string TargetEntityType, Guid TargetEntityId)
    : ICommand, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.tag.manage" };
    public string EntityType => nameof(Domain.Tags.EntityTag);
    public Guid EntityId => TagId;
    public string Action => "Detached";
}
