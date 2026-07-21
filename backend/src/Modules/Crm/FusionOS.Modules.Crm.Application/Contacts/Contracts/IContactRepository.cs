namespace FusionOS.Modules.Crm.Application.Contacts.Contracts;

public interface IContactRepository
{
    Task<Domain.Contacts.Contact?> GetByIdAsync(Guid companyId, Guid contactId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Contacts.Contact contact, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Contacts.Contact>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
