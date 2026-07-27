namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

/// <summary>
/// One Released work order's contribution to the company-wide WIP total — see
/// GetWorkInProgressSummaryReportQuery's doc comment for the valuation basis.
/// </summary>
public sealed record WorkInProgressLineDto(
    Guid WorkOrderId,
    Guid ProductId,
    decimal QuantityToProduce,
    decimal TotalIssuedValue);

/// <summary>
/// Work-in-progress valuation report — every Released work order's currently-issued
/// material value, and the company-wide total. See GetWorkInProgressSummaryReportQuery's
/// doc comment for exactly what this does and doesn't account for.
/// </summary>
public sealed record WorkInProgressSummaryReportDto(
    IReadOnlyList<WorkInProgressLineDto> Lines,
    decimal TotalWipValue,
    string ValuationNote);
