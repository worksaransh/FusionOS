namespace FusionOS.Modules.Quality.Application.Inspections.Contracts;

/// <summary>One recorded measurement for the queried characteristic — RecordedAt is the owning Inspection's CreatedAt (see doc comment on the query for why).</summary>
public sealed record SpcDataPointDto(Guid InspectionId, DateTimeOffset RecordedAt, decimal MeasuredValue);

/// <summary>
/// Result of GetSpcDataForCharacteristicQuery — see that query's doc comment for the full,
/// deliberate scope decision behind this shape (raw data points only, no Cp/Cpk).
/// </summary>
public sealed record SpcDataForCharacteristicDto(
    string CharacteristicName,
    DateTimeOffset? PeriodStart,
    DateTimeOffset? PeriodEnd,
    IReadOnlyList<SpcDataPointDto> DataPoints,
    decimal? LowerControlLimit,
    decimal? UpperControlLimit,
    string ScopeNote);
