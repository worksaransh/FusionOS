using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.QualifyLead;

public sealed record QualifyLeadCommand(Guid CompanyId, Guid LeadId)
    : ICommand<LeadDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.lead.qualify" };
    public string EntityType => nameof(Domain.Leads.Lead);
    public Guid EntityId => LeadId;
    public string Action => "Qualified";
}
