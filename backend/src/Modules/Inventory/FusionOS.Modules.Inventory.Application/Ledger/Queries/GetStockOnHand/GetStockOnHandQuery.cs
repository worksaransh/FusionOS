using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;

namespace FusionOS.Modules.Inventory.Application.Ledger.Queries.GetStockOnHand;

public sealed record GetStockOnHandQuery(Guid CompanyId, Guid ProductId, Guid? WarehouseId) : IQuery<StockOnHandDto>;
