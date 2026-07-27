using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;

namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Queries.ListSupplierRatingsBySupplier;

public sealed record ListSupplierRatingsBySupplierQuery(Guid CompanyId, Guid SupplierId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<SupplierRatingDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.supplier-rating.read" };
}
