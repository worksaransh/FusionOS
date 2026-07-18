namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

public sealed record BomLineDto(Guid Id, Guid ComponentProductId, decimal Quantity);

public sealed record BillOfMaterialsDto(
    Guid Id,
    string Code,
    string Name,
    Guid ProductId,
    bool IsActive,
    IReadOnlyList<BomLineDto> Lines);
