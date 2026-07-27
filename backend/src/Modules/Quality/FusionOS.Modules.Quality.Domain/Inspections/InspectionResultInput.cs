namespace FusionOS.Modules.Quality.Domain.Inspections;

/// <summary>
/// Input shape for recording one characteristic's result on an inspection — kept in Domain so
/// both Application and tests reference one definition.
///
/// <see cref="Passed"/> is nullable (was a plain <c>bool</c> before the SPC slice): pass null
/// to let <see cref="InspectionItem.RecordResult"/> auto-derive it from
/// <see cref="MeasuredValue"/> against the characteristic's control limits — see that method's
/// doc comment for the exact resolution rule (an explicit Passed always wins when supplied).
/// <see cref="MeasuredValue"/> is optional and trailing so existing pass/fail-only call sites
/// (positional `new InspectionResultInput(characteristic, passed, notes)`) keep compiling
/// unchanged.
/// </summary>
public sealed record InspectionResultInput(string Characteristic, bool? Passed, string? Notes, decimal? MeasuredValue = null);
