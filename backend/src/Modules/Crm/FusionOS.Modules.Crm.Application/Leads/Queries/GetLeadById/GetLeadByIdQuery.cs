using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;

namespace FusionOS.Modules.Crm.Application.Leads.Queries.GetLeadById;

public sealed record GetLeadByIdQuery(Guid CompanyId, Guid LeadId) : IQuery<LeadDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.lead.read" };
}
