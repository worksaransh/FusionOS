namespace FusionOS.Modules.Crm.Application.Opportunities.Contracts;

public sealed record OpportunityDto(
    Guid Id,
    Guid LeadId,
    string Name,
    string CustomerName,
    string? ContactEmail,
    decimal EstimatedValue,
    string Stage,
    string? CustomerCode,
    Guid? AccountId);

/// <summary>Single place that turns an Opportunity aggregate into its DTO, shared by every handler that returns one.</summary>
public static class OpportunityMapper
{
    public static OpportunityDto ToDto(Domain.Opportunities.Opportunity opportunity) => new(
        opportunity.Id,
        opportunity.LeadId,
        opportunity.Name,
        opportunity.CustomerName,
        opportunity.ContactEmail,
        opportunity.EstimatedValue,
        opportunity.Stage.ToString(),
        opportunity.CustomerCode,
        opportunity.AccountId);
}
