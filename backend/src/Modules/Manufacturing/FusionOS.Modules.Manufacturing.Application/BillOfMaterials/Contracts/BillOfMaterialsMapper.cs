namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

/// <summary>Single place that turns a BillOfMaterials aggregate into its DTO, shared by every handler that returns one.</summary>
public static class BillOfMaterialsMapper
{
    public static BillOfMaterialsDto ToDto(Domain.BillOfMaterials.BillOfMaterials bom) => new(
        bom.Id,
        bom.Code,
        bom.Name,
        bom.ProductId,
        bom.IsActive,
        bom.Lines.Select(l => new BomLineDto(l.Id, l.ComponentProductId, l.Quantity)).ToList());
}
