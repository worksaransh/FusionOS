using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Documents.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Documents.Commands.DeleteDocument;

public sealed class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Unit>
{
    private readonly IDocumentRepository _documents;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDocumentCommandHandler(IDocumentRepository documents, ICurrentUserContext currentUser, IUnitOfWork unitOfWork)
    {
        _documents = documents;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var actingUserId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var document = await _documents.GetByIdAsync(request.CompanyId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Document {request.Id} not found.");

        document.MarkDeleted(DateTimeOffset.UtcNow, actingUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
