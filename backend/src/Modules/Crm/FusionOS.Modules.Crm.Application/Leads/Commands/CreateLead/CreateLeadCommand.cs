using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.CreateLead;

public sealed record CreateLeadCommand(Guid CompanyId, string Name, string? ContactEmail, string? ContactPhone, string? Source)
    : ICommand<LeadDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.lead.create" };
    public string EntityType => nameof(Domain.Leads.Lead);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
