namespace FusionOS.Modules.Core.Application.Documents.Contracts;

public interface IDocumentRepository
{
    Task AddAsync(Domain.Documents.Document document, CancellationToken cancellationToken = default);

    Task<Domain.Documents.Document?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Metadata-only projection (2026-07-21 fix) - deliberately does NOT return
    /// full Document entities, so a listing request never pulls every attached
    /// file's full byte[] Content over the wire just to discard it in the DTO
    /// mapper. Matches DocumentDto's fields exactly.
    /// </summary>
    Task<IReadOnlyList<(Guid Id, string EntityType, Guid EntityId, string FileName, string ContentType, long FileSizeBytes, Guid UploadedByUserId, DateTimeOffset UploadedAt)>>
        ListByEntityAsync(Guid companyId, string entityType, Guid entityId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountByEntityAsync(Guid companyId, string entityType, Guid entityId, CancellationToken cancellationToken = default);
}
