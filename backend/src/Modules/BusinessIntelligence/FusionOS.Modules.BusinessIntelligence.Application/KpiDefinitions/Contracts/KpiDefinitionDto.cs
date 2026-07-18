namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;

public sealed record KpiDefinitionDto(Guid Id, string Code, string Name, string? Unit, bool IsActive);

/// <summary>Single place that turns a KpiDefinition aggregate into its DTO, shared by every handler that returns one.</summary>
public static class KpiDefinitionMapper
{
    public static KpiDefinitionDto ToDto(Domain.KpiDefinitions.KpiDefinition kpi) =>
        new(kpi.Id, kpi.Code, kpi.Name, kpi.Unit, kpi.IsActive);
}
