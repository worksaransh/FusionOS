using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.CreditNotes.Commands.CreateCreditNote;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.CreditNotes.Commands.IssueCreditNote;

public sealed class IssueCreditNoteCommandHandler : IRequestHandler<IssueCreditNoteCommand, CreditNoteDto>
{
    private readonly ICreditNoteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public IssueCreditNoteCommandHandler(ICreditNoteRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreditNoteDto> Handle(IssueCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var creditNote = await _repository.GetByIdAsync(request.CompanyId, request.CreditNoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Credit note '{request.CreditNoteId}' was not found.");

        creditNote.Issue();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateCreditNoteCommandHandler.MapToDto(creditNote);
    }
}
