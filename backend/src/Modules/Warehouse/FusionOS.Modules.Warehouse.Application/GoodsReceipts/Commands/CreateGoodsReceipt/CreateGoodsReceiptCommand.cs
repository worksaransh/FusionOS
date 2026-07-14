using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.CreateGoodsReceipt;

public sealed record CreateGoodsReceiptCommand(
    Guid CompanyId,
    Guid WarehouseId,
    Guid ZoneId,
    Guid? PurchaseOrderId,
    Guid? SupplierId,
    IReadOnlyList<GoodsReceiptLineInput> Lines)
    : ICommand<GoodsReceiptDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.goods-receipt.create" };
    public string EntityType => nameof(Domain.GoodsReceipts.GoodsReceipt);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
