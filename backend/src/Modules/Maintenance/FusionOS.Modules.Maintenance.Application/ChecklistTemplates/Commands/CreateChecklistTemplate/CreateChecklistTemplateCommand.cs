using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.CreateChecklistTemplate;

public sealed record CreateChecklistTemplateCommand(Guid CompanyId, string Name, string? Description, IReadOnlyList<string> StepDescriptions)
    : ICommand<ChecklistTemplateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.checklist-template.create" };
    public string EntityType => nameof(Domain.ChecklistTemplates.ChecklistTemplate);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
