using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Documents.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Documents.Commands.UploadDocument;

public sealed class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documents;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UploadDocumentCommandHandler(IDocumentRepository documents, ICurrentUserContext currentUser, IUnitOfWork unitOfWork)
    {
        _documents = documents;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var uploadedBy = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var document = Domain.Documents.Document.Upload(
            request.CompanyId,
            request.EntityType,
            request.EntityId,
            request.FileName,
            request.ContentType,
            request.Content,
            uploadedBy);

        await _documents.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DocumentDto(
            document.Id,
            document.EntityType,
            document.EntityId,
            document.FileName,
            document.ContentType,
            document.FileSizeBytes,
            document.UploadedByUserId,
            document.UploadedAt);
    }
}
