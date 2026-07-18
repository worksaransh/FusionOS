namespace FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;

/// <summary>Input shape for one BOM component line — kept in Domain so both Application and tests reference one definition.</summary>
public sealed record BomLineInput(Guid ComponentProductId, decimal Quantity);
