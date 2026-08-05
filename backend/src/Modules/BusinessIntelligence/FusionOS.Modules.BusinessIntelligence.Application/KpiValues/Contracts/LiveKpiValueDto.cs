namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;

/// <summary>The live-computed value of a KPI, plus the timestamp it was computed at (the "as of" freshness stamp — Phase 2.3).</summary>
public sealed record LiveKpiValueDto(Guid KpiDefinitionId, string KpiCode, string KpiName, decimal Value, DateTimeOffset ComputedAt);
