using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.DeactivateChecklistTemplate;

/// <summary>
/// Reuses maintenance.checklist-template.create rather than a dedicated
/// "...deactivate" permission — no separate deactivate permission was reserved
/// for checklist templates in PermissionCatalog (unlike Asset/BillOfMaterials,
/// which each got their own create+deactivate pair), and this pass doesn't own
/// PermissionCatalog.cs to add one. A checklist template's lifecycle is fully
/// managed by whoever is allowed to create/edit templates in the first place,
/// so gating deactivate behind "create" is a reasonable, documented choice
/// until a follow-up reserves a dedicated permission if finer-grained control
/// is ever needed.
/// </summary>
public sealed record DeactivateChecklistTemplateCommand(Guid CompanyId, Guid ChecklistTemplateId)
    : ICommand<ChecklistTemplateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.checklist-template.create" };
    public string EntityType => nameof(Domain.ChecklistTemplates.ChecklistTemplate);
    public Guid EntityId => ChecklistTemplateId;
    public string Action => "Deactivated";
}
