namespace FusionOS.Modules.Crm.Application.Dashboard.Contracts;

/// <summary>One non-terminal pipeline stage's aggregate value, for the CRM dashboard's pipeline-by-stage breakdown.</summary>
public sealed record PipelineStageDto(string Stage, decimal EstimatedValueTotal, decimal WeightedValueTotal);

/// <summary>
/// CRM's canned dashboard — pipeline value by (open) stage, win rate among ever-closed
/// opportunities, and lead conversion rate. Mirrors Finance's Reports DTOs shape (e.g.
/// ApAgingReportDto): one flat, pre-aggregated read-model record, no lazy nested queries.
/// </summary>
public sealed record CrmDashboardDto(
    IReadOnlyList<PipelineStageDto> PipelineByStage,
    decimal TotalPipelineEstimatedValue,
    decimal TotalPipelineWeightedValue,
    int WonCount,
    int LostCount,
    decimal WinRatePercent,
    int TotalLeadCount,
    int ConvertedLeadCount,
    decimal LeadConversionRatePercent);
