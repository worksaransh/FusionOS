using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;

namespace FusionOS.Modules.Crm.Application.Leads.Queries.ListLeads;

public sealed record ListLeadsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<LeadDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.lead.read" };
}
