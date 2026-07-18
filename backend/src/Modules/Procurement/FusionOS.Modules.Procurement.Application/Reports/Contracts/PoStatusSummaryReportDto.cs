namespace FusionOS.Modules.Procurement.Application.Reports.Contracts;

/// <summary>Count of purchase orders in one status (Phase M6, 2026-07-15).</summary>
public sealed record PoStatusSummaryLineDto(string Status, int Count);

public sealed record PoStatusSummaryReportDto(IReadOnlyList<PoStatusSummaryLineDto> Lines, int TotalCount);
