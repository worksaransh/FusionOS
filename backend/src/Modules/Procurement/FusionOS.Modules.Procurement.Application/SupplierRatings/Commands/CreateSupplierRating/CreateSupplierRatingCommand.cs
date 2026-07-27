using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;

namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Commands.CreateSupplierRating;

public sealed record CreateSupplierRatingCommand(
    Guid CompanyId,
    Guid SupplierId,
    Guid? PurchaseOrderId,
    int QualityScore,
    int DeliveryScore,
    Guid RatedByUserId,
    string? Comments)
    : ICommand<SupplierRatingDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.supplier-rating.create" };
    public string EntityType => nameof(Domain.SupplierRatings.SupplierRating);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
