namespace FusionOS.Modules.Warehouse.Application.Packages.Contracts;

/// <summary>
/// Shared domain-to-DTO mapper — same "lives here once" rationale as PickListMapper: Create/
/// GetById/ListByPickList handlers all need the identical mapping.
/// </summary>
public static class PackageMapper
{
    public static PackageDto MapToDto(Domain.Packages.Package package) => new(
        package.Id,
        package.PickListId,
        package.PackageNumber,
        package.WeightKg,
        package.LengthCm,
        package.WidthCm,
        package.HeightCm,
        package.Lines.Select(l => new PackageLineDto(l.Id, l.ProductId, l.Quantity)).ToList(),
        package.CreatedAt);
}
