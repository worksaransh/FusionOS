namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

/// <summary>One demand line an MRP run is planning for — "we need Quantity units of ProductId", independent of where that demand came from (a Sales Order, a forecast, a manual planning entry — MRP itself is source-agnostic; see GenerateMrpRequirementsQuery's doc comment for what building that demand list from real orders would additionally require).</summary>
public sealed record MrpDemandLine(Guid ProductId, decimal Quantity);

/// <summary>One line of an MRP run's gross requirements — see GenerateMrpRequirementsQuery's doc comment for exactly what "gross" means here (not netted against on-hand stock or incoming supply).</summary>
public sealed record MrpRequirementLineDto(Guid ProductId, decimal TotalQuantityRequired);

public sealed record MrpRequirementsResultDto(
    IReadOnlyList<MrpRequirementLineDto> Requirements,
    IReadOnlyList<string> Warnings);
