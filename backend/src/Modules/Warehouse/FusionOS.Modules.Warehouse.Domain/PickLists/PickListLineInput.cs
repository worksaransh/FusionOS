namespace FusionOS.Modules.Warehouse.Domain.PickLists;

/// <summary>Input shape for PickList.Create — mirrors GoodsReceiptLineInput/SalesOrderLineInput; never touches the database itself.</summary>
public sealed record PickListLineInput(Guid ProductId, Guid? BinId, decimal QuantityToPick);
