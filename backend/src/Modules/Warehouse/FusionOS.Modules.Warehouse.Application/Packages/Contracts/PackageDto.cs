namespace FusionOS.Modules.Warehouse.Application.Packages.Contracts;

public sealed record PackageLineDto(Guid Id, Guid ProductId, decimal Quantity);

public sealed record PackageDto(
    Guid Id,
    Guid PickListId,
    string PackageNumber,
    decimal? WeightKg,
    decimal? LengthCm,
    decimal? WidthCm,
    decimal? HeightCm,
    IReadOnlyList<PackageLineDto> Lines,
    DateTimeOffset CreatedAt);
