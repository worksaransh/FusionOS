using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Documents.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Documents.Queries.ListDocumentsByEntity;

public sealed class ListDocumentsByEntityQueryHandler : IRequestHandler<ListDocumentsByEntityQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _repository;

    public ListDocumentsByEntityQueryHandler(IDocumentRepository repository) => _repository = repository;

    public async Task<PagedResult<DocumentDto>> Handle(ListDocumentsByEntityQuery request, CancellationToken cancellationToken)
    {
        var documents = await _repository.ListByEntityAsync(request.CompanyId, request.EntityType, request.EntityId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByEntityAsync(request.CompanyId, request.EntityType, request.EntityId, cancellationToken);

        var dtos = documents.Select(MapToDto).ToList();
        return new PagedResult<DocumentDto>(dtos, request.Page, request.PageSize, total);
    }

    internal static DocumentDto MapToDto((Guid Id, string EntityType, Guid EntityId, string FileName, string ContentType, long FileSizeBytes, Guid UploadedByUserId, DateTimeOffset UploadedAt) row) => new(
        row.Id,
        row.EntityType,
        row.EntityId,
        row.FileName,
        row.ContentType,
        row.FileSizeBytes,
        row.UploadedByUserId,
        row.UploadedAt);
}
