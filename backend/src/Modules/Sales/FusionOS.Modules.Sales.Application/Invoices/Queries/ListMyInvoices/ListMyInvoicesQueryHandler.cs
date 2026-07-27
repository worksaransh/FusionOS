using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Invoices.Commands.CreateInvoice;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Invoices.Queries.ListMyInvoices;

public sealed class ListMyInvoicesQueryHandler : IRequestHandler<ListMyInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly IInvoiceRepository _repository;

    public ListMyInvoicesQueryHandler(IInvoiceRepository repository) => _repository = repository;

    public async Task<PagedResult<InvoiceDto>> Handle(ListMyInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _repository.ListByCustomerAsync(request.CustomerId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByCustomerAsync(request.CustomerId, cancellationToken);

        var dtos = invoices.Select(CreateInvoiceCommandHandler.MapToDto).ToList();

        return new PagedResult<InvoiceDto>(dtos, request.Page, request.PageSize, total);
    }
}
