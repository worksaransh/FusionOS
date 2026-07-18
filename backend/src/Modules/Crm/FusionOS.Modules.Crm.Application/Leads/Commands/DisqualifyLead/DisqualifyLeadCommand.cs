using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.DisqualifyLead;

public sealed record DisqualifyLeadCommand(Guid CompanyId, Guid LeadId)
    : ICommand<LeadDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.lead.disqualify" };
    public string EntityType => nameof(Domain.Leads.Lead);
    public Guid EntityId => LeadId;
    public string Action => "Disqualified";
}
