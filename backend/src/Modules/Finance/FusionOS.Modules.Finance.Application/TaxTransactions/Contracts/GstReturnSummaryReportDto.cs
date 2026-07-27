namespace FusionOS.Modules.Finance.Application.TaxTransactions.Contracts;

/// <summary>One row per GST component (CGST/SGST/IGST) actually present in the period's TaxTransaction data — a component with zero activity is simply omitted, not zero-padded.</summary>
public sealed record GstReturnComponentLineDto(
    string GstComponent,
    decimal OutputTaxCollected,
    decimal InputTaxCredit,
    decimal NetPayable);

/// <summary>
/// GSTR-1/GSTR-3B-style period return. Unlike GetTaxPeriodSummaryReportQuery
/// (the pre-existing rate-register scaffold left with null tax amounts because
/// no event carried real figures), this report reads real TaxTransaction rows —
/// populated by InvoiceIssuedConsumer (Direction=Output) and
/// PurchaseOrderGoodsReceiptCostedConsumer (Direction=Input) once those events
/// carry TaxRateId/TaxAmount. NetPayable per component is OutputTaxCollected
/// minus InputTaxCredit (can be negative, meaning a carried-forward credit).
/// </summary>
public sealed record GstReturnSummaryReportDto(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    IReadOnlyList<GstReturnComponentLineDto> Components,
    decimal TotalOutputTaxCollected,
    decimal TotalInputTaxCredit,
    decimal TotalNetPayable);
