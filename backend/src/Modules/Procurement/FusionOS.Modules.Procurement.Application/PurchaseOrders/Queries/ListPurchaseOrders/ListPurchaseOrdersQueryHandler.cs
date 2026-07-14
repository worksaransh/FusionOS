using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;

public sealed class ListPurchaseOrdersQueryHandler : IRequestHandler<ListPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    private readonly IPurchaseOrderRepository _repository;

    public ListPurchaseOrdersQueryHandler(IPurchaseOrderRepository repository) => _repository = repository;

    public async Task<PagedResult<PurchaseOrderDto>> Handle(ListPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = orders.Select(CreatePurchaseOrderCommandHandler.MapToDto).ToList();

        return new PagedResult<PurchaseOrderDto>(dtos, request.Page, request.PageSize, total);
    }
}
