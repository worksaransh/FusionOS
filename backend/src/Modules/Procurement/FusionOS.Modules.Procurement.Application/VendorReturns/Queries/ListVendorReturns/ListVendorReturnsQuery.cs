using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Queries.ListVendorReturns;

public sealed record ListVendorReturnsQuery(Guid CompanyId, Guid? PurchaseOrderId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<VendorReturnDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.vendor-return.read" };
}
