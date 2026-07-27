using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListMySalesOrders;

public sealed class ListMySalesOrdersQueryHandler : IRequestHandler<ListMySalesOrdersQuery, PagedResult<SalesOrderDto>>
{
    private readonly ISalesOrderRepository _repository;

    public ListMySalesOrdersQueryHandler(ISalesOrderRepository repository) => _repository = repository;

    public async Task<PagedResult<SalesOrderDto>> Handle(ListMySalesOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.ListByCustomerAsync(request.CustomerId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByCustomerAsync(request.CustomerId, cancellationToken);

        var dtos = orders.Select(CreateSalesOrderCommandHandler.MapToDto).ToList();

        return new PagedResult<SalesOrderDto>(dtos, request.Page, request.PageSize, total);
    }
}
