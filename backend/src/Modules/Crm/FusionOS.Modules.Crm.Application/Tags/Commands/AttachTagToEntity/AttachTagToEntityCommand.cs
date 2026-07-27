using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tags.Contracts;

namespace FusionOS.Modules.Crm.Application.Tags.Commands.AttachTagToEntity;

/// <summary>
/// Attaches a Tag to a (TargetEntityType, TargetEntityId) pair — a Lead, Contact,
/// Account, or Opportunity. Named TargetEntityType/TargetEntityId (rather than
/// EntityType/EntityId) to avoid colliding with IAuditableCommand's own EntityType/
/// EntityId, which here identify the audited EntityTag record itself.
/// </summary>
public sealed record AttachTagToEntityCommand(Guid CompanyId, Guid TagId, string TargetEntityType, Guid TargetEntityId)
    : ICommand<TagAttachmentDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.tag.manage" };
    public string EntityType => nameof(Domain.Tags.EntityTag);
    public Guid EntityId => TagId;
    public string Action => "Attached";
}
