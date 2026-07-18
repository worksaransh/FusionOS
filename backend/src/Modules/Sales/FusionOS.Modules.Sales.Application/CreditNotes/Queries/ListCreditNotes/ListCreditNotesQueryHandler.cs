using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.CreditNotes.Commands.CreateCreditNote;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.CreditNotes.Queries.ListCreditNotes;

public sealed class ListCreditNotesQueryHandler : IRequestHandler<ListCreditNotesQuery, PagedResult<CreditNoteDto>>
{
    private readonly ICreditNoteRepository _repository;

    public ListCreditNotesQueryHandler(ICreditNoteRepository repository) => _repository = repository;

    public async Task<PagedResult<CreditNoteDto>> Handle(ListCreditNotesQuery request, CancellationToken cancellationToken)
    {
        var creditNotes = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = creditNotes.Select(CreateCreditNoteCommandHandler.MapToDto).ToList();

        return new PagedResult<CreditNoteDto>(dtos, request.Page, request.PageSize, total);
    }
}
