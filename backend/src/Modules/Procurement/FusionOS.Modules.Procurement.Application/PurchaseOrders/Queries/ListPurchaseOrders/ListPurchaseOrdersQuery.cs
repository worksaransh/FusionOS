using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;

public sealed record ListPurchaseOrdersQuery(Guid CompanyId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<PurchaseOrderDto>>;
