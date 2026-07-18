using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.ConfirmPutaway;

public sealed record ConfirmPutawayCommand(Guid CompanyId, Guid GoodsReceiptId, Guid LineId, Guid BinId)
    : ICommand<GoodsReceiptDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.goods-receipt.confirm-putaway" };
    public string EntityType => nameof(Domain.GoodsReceipts.GoodsReceipt);
    public Guid EntityId => GoodsReceiptId;
    public string Action => "PutawayConfirmed";
}
