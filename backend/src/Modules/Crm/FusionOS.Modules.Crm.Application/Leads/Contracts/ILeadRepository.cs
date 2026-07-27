namespace FusionOS.Modules.Crm.Application.Leads.Contracts;

public interface ILeadRepository
{
    Task<Domain.Leads.Lead?> GetByIdAsync(Guid companyId, Guid leadId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Leads.Lead lead, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Leads.Lead>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>Server-side counts of total leads vs Converted leads, for the CRM dashboard's lead-conversion-rate metric.</summary>
    Task<(int TotalCount, int ConvertedCount)> GetConversionCountsAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Advisory duplicate check for CreateLeadCommandHandler — does any lead in this company
    /// already carry this exact normalized (trimmed, lower-cased) contact email? Callers
    /// only use this to populate LeadDto.IsPossibleDuplicate as a warning; it is never used
    /// to block creation (see CreateLeadCommandHandler for why).
    /// </summary>
    Task<bool> IsEmailAlreadyUsedAsync(Guid companyId, string normalizedEmail, CancellationToken cancellationToken = default);
}
