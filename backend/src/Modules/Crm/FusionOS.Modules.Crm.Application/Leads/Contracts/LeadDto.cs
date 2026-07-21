namespace FusionOS.Modules.Crm.Application.Leads.Contracts;

public sealed record LeadDto(
    Guid Id,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? Source,
    string Status,
    Guid? AccountId);

/// <summary>Single place that turns a Lead aggregate into its DTO, shared by every handler that returns one.</summary>
public static class LeadMapper
{
    public static LeadDto ToDto(Domain.Leads.Lead lead) =>
        new(lead.Id, lead.Name, lead.ContactEmail, lead.ContactPhone, lead.Source, lead.Status.ToString(), lead.AccountId);
}
