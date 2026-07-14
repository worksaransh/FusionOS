using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Invoices.Commands.CreateInvoice;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Invoices.Queries.ListInvoices;

public sealed class ListInvoicesQueryHandler : IRequestHandler<ListInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly IInvoiceRepository _repository;

    public ListInvoicesQueryHandler(IInvoiceRepository repository) => _repository = repository;

    public async Task<PagedResult<InvoiceDto>> Handle(ListInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = invoices.Select(CreateInvoiceCommandHandler.MapToDto).ToList();

        return new PagedResult<InvoiceDto>(dtos, request.Page, request.PageSize, total);
    }
}
