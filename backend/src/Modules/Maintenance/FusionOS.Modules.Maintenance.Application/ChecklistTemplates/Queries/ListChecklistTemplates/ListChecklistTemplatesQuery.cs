using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Queries.ListChecklistTemplates;

public sealed record ListChecklistTemplatesQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ChecklistTemplateDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.checklist-template.read" };
}
