namespace FusionOS.Modules.Crm.Application.Leads.Contracts;

public sealed record LeadDto(
    Guid Id,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? Source,
    string Status,
    Guid? AccountId,
    Guid? OwnerId,
    int Score,
    bool IsPossibleDuplicate = false);

/// <summary>Single place that turns a Lead aggregate into its DTO, shared by every handler that returns one.</summary>
public static class LeadMapper
{
    /// <summary>Plain mapping with no duplicate-warning context — used by every read path (List/GetById) where the concept doesn't apply.</summary>
    public static LeadDto ToDto(Domain.Leads.Lead lead) => ToDto(lead, isPossibleDuplicate: false);

    /// <summary>
    /// Used by CreateLeadCommandHandler to attach the advisory IsPossibleDuplicate flag it
    /// computed before creating (see the handler for why this warns rather than blocks).
    /// </summary>
    public static LeadDto ToDto(Domain.Leads.Lead lead, bool isPossibleDuplicate) =>
        new(lead.Id, lead.Name, lead.ContactEmail, lead.ContactPhone, lead.Source, lead.Status.ToString(), lead.AccountId, lead.OwnerId, lead.Score, isPossibleDuplicate);
}
