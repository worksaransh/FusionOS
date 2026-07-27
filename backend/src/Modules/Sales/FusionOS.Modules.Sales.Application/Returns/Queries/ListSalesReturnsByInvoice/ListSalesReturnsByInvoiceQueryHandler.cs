using FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Returns.Queries.ListSalesReturnsByInvoice;

public sealed class ListSalesReturnsByInvoiceQueryHandler : IRequestHandler<ListSalesReturnsByInvoiceQuery, IReadOnlyList<SalesReturnDto>>
{
    private readonly ISalesReturnRepository _repository;

    public ListSalesReturnsByInvoiceQueryHandler(ISalesReturnRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<SalesReturnDto>> Handle(ListSalesReturnsByInvoiceQuery request, CancellationToken cancellationToken)
    {
        var salesReturns = await _repository.ListByInvoiceAsync(request.CompanyId, request.InvoiceId, cancellationToken);
        return salesReturns.Select(CreateSalesReturnCommandHandler.MapToDto).ToList();
    }
}
