using FusionOS.Modules.Core.Application.Documents.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Documents.Queries.DownloadDocument;

public sealed class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, DocumentContentDto>
{
    private readonly IDocumentRepository _repository;

    public DownloadDocumentQueryHandler(IDocumentRepository repository) => _repository = repository;

    public async Task<DocumentContentDto> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.CompanyId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Document {request.Id} not found.");

        return new DocumentContentDto(document.FileName, document.ContentType, document.Content);
    }
}
