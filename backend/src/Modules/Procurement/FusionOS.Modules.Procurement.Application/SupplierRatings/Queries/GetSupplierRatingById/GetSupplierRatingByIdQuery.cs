using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;

namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Queries.GetSupplierRatingById;

public sealed record GetSupplierRatingByIdQuery(Guid CompanyId, Guid SupplierRatingId)
    : IQuery<SupplierRatingDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.supplier-rating.read" };
}
