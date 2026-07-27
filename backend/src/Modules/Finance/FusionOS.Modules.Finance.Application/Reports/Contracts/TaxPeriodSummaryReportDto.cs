namespace FusionOS.Modules.Finance.Application.Reports.Contracts;

/// <summary>
/// One TaxRate's line on the period summary, nested under its TaxJurisdiction.
/// OutputTaxCollected/InputTaxPaid/NetTaxPayable are deliberately nullable —
/// this query never computes them (it's a rate/jurisdiction register only),
/// so null honestly states "not computed by this report" rather than a stored
/// zero misleadingly claiming "no tax was collected/paid against this rate
/// during the period." For actual computed amounts, see
/// GetGstReturnSummaryReportQuery, which derives them from Finance's
/// TaxTransaction records.
/// </summary>
public sealed record TaxPeriodSummaryLineDto(
    Guid TaxJurisdictionId,
    string TaxJurisdictionCode,
    string TaxJurisdictionName,
    Guid TaxRateId,
    string TaxRateCode,
    string TaxRateName,
    decimal Percentage,
    decimal? OutputTaxCollected,
    decimal? InputTaxPaid,
    decimal? NetTaxPayable);

/// <summary>
/// Tax period summary report — see GetTaxPeriodSummaryReportQuery's doc
/// comment for the full scope decision behind why this is a tax rate register
/// rather than a real filed-amounts report today. Lines cover every active
/// TaxRate under every active TaxJurisdiction for the company as of now — not
/// filtered by PeriodStart/PeriodEnd, since master data has no transaction
/// date to filter by. PeriodStart/PeriodEnd are still accepted and echoed
/// back, both to keep the query shape identical to what a real filing report
/// will need once real amounts are wired in, and so a Finance user requesting
/// this for "Q2" sees confirmation of which period they asked about even
/// though the amounts for it aren't derivable yet. DataAvailabilityNote states
/// the gap in the same words a Finance user reading this report in the UI
/// needs, so the limitation is visible at the point of use, not just buried
/// in a code comment.
/// </summary>
public sealed record TaxPeriodSummaryReportDto(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    IReadOnlyList<TaxPeriodSummaryLineDto> Lines,
    decimal? TotalOutputTaxCollected,
    decimal? TotalInputTaxPaid,
    decimal? TotalNetTaxPayable,
    string DataAvailabilityNote);
