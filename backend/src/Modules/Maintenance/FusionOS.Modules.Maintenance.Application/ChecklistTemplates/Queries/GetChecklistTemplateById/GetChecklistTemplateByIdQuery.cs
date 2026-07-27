using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Queries.GetChecklistTemplateById;

public sealed record GetChecklistTemplateByIdQuery(Guid CompanyId, Guid ChecklistTemplateId) : IQuery<ChecklistTemplateDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.checklist-template.read" };
}
