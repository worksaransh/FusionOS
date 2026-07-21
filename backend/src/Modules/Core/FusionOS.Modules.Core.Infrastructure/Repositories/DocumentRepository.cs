using FusionOS.Modules.Core.Application.Documents.Contracts;
using FusionOS.Modules.Core.Domain.Documents;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly CoreDbContext _context;

    public DocumentRepository(CoreDbContext context) => _context = context;

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default) =>
        await _context.Documents.AddAsync(document, cancellationToken);

    public Task<Document?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.Documents.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    // Metadata-only projection (2026-07-21 fix) - selects only the 8 columns
    // DocumentDto needs, so a listing request never pulls Document.Content
    // (the file's full byte[], up to Document.MaxFileSizeBytes each) over the
    // wire just to have the handler's DTO mapper discard it immediately.
    public async Task<IReadOnlyList<(Guid Id, string EntityType, Guid EntityId, string FileName, string ContentType, long FileSizeBytes, Guid UploadedByUserId, DateTimeOffset UploadedAt)>>
        ListByEntityAsync(Guid companyId, string entityType, Guid entityId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var rows = await Filtered(companyId, entityType, entityId)
            .OrderByDescending(x => x.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new { x.Id, x.EntityType, x.EntityId, x.FileName, x.ContentType, x.FileSizeBytes, x.UploadedByUserId, x.UploadedAt })
            .ToListAsync(cancellationToken);

        return rows.Select(x => (x.Id, x.EntityType, x.EntityId, x.FileName, x.ContentType, x.FileSizeBytes, x.UploadedByUserId, x.UploadedAt)).ToList();
    }

    public Task<int> CountByEntityAsync(Guid companyId, string entityType, Guid entityId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, entityType, entityId).CountAsync(cancellationToken);

    private IQueryable<Document> Filtered(Guid companyId, string entityType, Guid entityId) =>
        _context.Documents.Where(d => d.CompanyId == companyId && d.EntityType == entityType && d.EntityId == entityId);
}
