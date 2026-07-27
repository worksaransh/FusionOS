namespace FusionOS.Modules.Crm.Application.Contacts.Contracts;

public interface IContactRepository
{
    Task<Domain.Contacts.Contact?> GetByIdAsync(Guid companyId, Guid contactId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Contacts.Contact contact, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Contacts.Contact>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Advisory duplicate check for CreateContactCommandHandler — does any active contact in
    /// this company already carry this exact normalized (trimmed, lower-cased) email? Used
    /// only to populate ContactDto.IsPossibleDuplicate as a warning, never to block creation.
    /// </summary>
    Task<bool> IsEmailAlreadyUsedAsync(Guid companyId, string normalizedEmail, CancellationToken cancellationToken = default);
}
