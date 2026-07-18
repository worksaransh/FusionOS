namespace FusionOS.Modules.Inventory.Application.Transfers.Contracts;

public sealed record TransferDto(Guid Id, Guid ProductId, Guid SourceWarehouseId, Guid DestinationWarehouseId, decimal Quantity, string Status, DateTimeOffset TransferDate);

/// <summary>Single place that turns a Transfer aggregate into its DTO, shared by every handler that returns one.</summary>
public static class TransferMapper
{
    public static TransferDto ToDto(Domain.Transfers.Transfer transfer) => new(
        transfer.Id, transfer.ProductId, transfer.SourceWarehouseId, transfer.DestinationWarehouseId,
        transfer.Quantity, transfer.Status.ToString(), transfer.TransferDate);
}
