namespace FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;

public sealed record SerialUnitDto(Guid Id, Guid ProductId, string SerialNumber, string Status, DateTimeOffset ReceivedAt);

/// <summary>Single place that turns a SerialUnit aggregate into its DTO, shared by every handler that returns one — same convention as ReservationMapper/BatchMapper.</summary>
public static class SerialUnitMapper
{
    public static SerialUnitDto ToDto(Domain.SerialUnits.SerialUnit unit) => new(
        unit.Id, unit.ProductId, unit.SerialNumber, unit.Status.ToString(), unit.ReceivedAt);
}
