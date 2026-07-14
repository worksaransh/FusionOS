using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Queries.ListGoodsReceipts;

public sealed record ListGoodsReceiptsQuery(Guid CompanyId, Guid WarehouseId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<GoodsReceiptDto>>;
