namespace FusionOS.Modules.Crm.Application.Leads.Contracts;

public interface ILeadRepository
{
    Task<Domain.Leads.Lead?> GetByIdAsync(Guid companyId, Guid leadId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Leads.Lead lead, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Leads.Lead>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
