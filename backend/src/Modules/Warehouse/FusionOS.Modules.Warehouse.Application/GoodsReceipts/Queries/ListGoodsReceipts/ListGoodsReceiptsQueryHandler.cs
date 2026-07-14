using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.CreateGoodsReceipt;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Queries.ListGoodsReceipts;

public sealed class ListGoodsReceiptsQueryHandler : IRequestHandler<ListGoodsReceiptsQuery, PagedResult<GoodsReceiptDto>>
{
    private readonly IGoodsReceiptRepository _repository;

    public ListGoodsReceiptsQueryHandler(IGoodsReceiptRepository repository) => _repository = repository;

    public async Task<PagedResult<GoodsReceiptDto>> Handle(ListGoodsReceiptsQuery request, CancellationToken cancellationToken)
    {
        var receipts = await _repository.ListAsync(request.CompanyId, request.WarehouseId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.WarehouseId, cancellationToken);

        var dtos = receipts.Select(CreateGoodsReceiptCommandHandler.MapToDto).ToList();

        return new PagedResult<GoodsReceiptDto>(dtos, request.Page, request.PageSize, total);
    }
}
