namespace FusionOS.Modules.Warehouse.Application.PickLists.Contracts;

/// <summary>
/// Shared domain-to-DTO mapper — every PickList command/query handler needs the identical mapping,
/// so it lives here once rather than duplicated per handler (unlike the single-handler-owns-the-map
/// convention used by e.g. GetApprovalRequestQueryHandler.MapToDto, which only had one obvious owner;
/// PickList has five handlers that equally need it).
/// </summary>
public static class PickListMapper
{
    public static PickListDto MapToDto(Domain.PickLists.PickList pickList) => new(
        pickList.Id,
        pickList.WarehouseId,
        pickList.SalesOrderId,
        pickList.AssignedToUserId,
        pickList.Status.ToString(),
        pickList.Lines.Select(l => new PickListLineDto(l.Id, l.ProductId, l.BinId, l.QuantityToPick, l.QuantityPicked)).ToList(),
        pickList.CreatedAt);
}
