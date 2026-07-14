using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListSalesOrders;

public sealed class ListSalesOrdersQueryHandler : IRequestHandler<ListSalesOrdersQuery, PagedResult<SalesOrderDto>>
{
    private readonly ISalesOrderRepository _repository;

    public ListSalesOrdersQueryHandler(ISalesOrderRepository repository) => _repository = repository;

    public async Task<PagedResult<SalesOrderDto>> Handle(ListSalesOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = orders.Select(CreateSalesOrderCommandHandler.MapToDto).ToList();

        return new PagedResult<SalesOrderDto>(dtos, request.Page, request.PageSize, total);
    }
}
