using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.UpdateChecklistTemplate;

public sealed record UpdateChecklistTemplateCommand(Guid CompanyId, Guid ChecklistTemplateId, string Name, string? Description, IReadOnlyList<string> StepDescriptions)
    : ICommand<ChecklistTemplateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.checklist-template.update" };
    public string EntityType => nameof(Domain.ChecklistTemplates.ChecklistTemplate);
    public Guid EntityId => ChecklistTemplateId;
    public string Action => "Updated";
}
