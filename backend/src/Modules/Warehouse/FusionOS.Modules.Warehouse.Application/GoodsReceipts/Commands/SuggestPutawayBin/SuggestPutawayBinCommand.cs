using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.SuggestPutawayBin;

public sealed record SuggestPutawayBinCommand(Guid CompanyId, Guid GoodsReceiptId, Guid LineId)
    : ICommand<GoodsReceiptDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.goods-receipt.suggest-putaway" };
    public string EntityType => nameof(Domain.GoodsReceipts.GoodsReceipt);
    public Guid EntityId => GoodsReceiptId;
    public string Action => "PutawaySuggested";
}
