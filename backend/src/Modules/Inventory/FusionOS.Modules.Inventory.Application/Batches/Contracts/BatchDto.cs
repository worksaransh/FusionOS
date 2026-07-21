namespace FusionOS.Modules.Inventory.Application.Batches.Contracts;

public sealed record BatchDto(Guid Id, Guid ProductId, string BatchNumber, DateTimeOffset? ExpiryDate, decimal QuantityReceived, decimal QuantityRemaining, DateTimeOffset CreatedAt);

/// <summary>Single place that turns a Batch aggregate into its DTO, shared by every handler that returns one — same convention as ReservationMapper.</summary>
public static class BatchMapper
{
    public static BatchDto ToDto(Domain.Batches.Batch batch) => new(
        batch.Id, batch.ProductId, batch.BatchNumber, batch.ExpiryDate, batch.QuantityReceived, batch.QuantityRemaining, batch.CreatedAt);
}
