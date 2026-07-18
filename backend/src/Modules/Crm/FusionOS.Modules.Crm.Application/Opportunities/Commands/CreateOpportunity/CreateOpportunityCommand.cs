using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.CreateOpportunity;

public sealed record CreateOpportunityCommand(Guid CompanyId, Guid LeadId, string Name, decimal EstimatedValue)
    : ICommand<OpportunityDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.create" };
    public string EntityType => nameof(Domain.Opportunities.Opportunity);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
