namespace FusionOS.Modules.Quality.Domain.Inspections;

/// <summary>
/// One characteristic checked as part of an <see cref="Inspection"/> (e.g. "Dimensional
/// tolerance", "Surface finish"). <see cref="Passed"/> is null until results are recorded.
/// Same documented no-audit/tenant-columns exception as every other line entity — its
/// lifecycle is owned entirely by the parent Inspection.
///
/// <see cref="MeasuredValue"/>/<see cref="LowerControlLimit"/>/<see cref="UpperControlLimit"/>
/// are the Statistical Process Control (SPC) slice: a characteristic can optionally carry a
/// numeric measurement instead of (or alongside) a plain pass/fail. All three are nullable —
/// a pass/fail-only characteristic (the original, pre-SPC shape) simply never sets them and
/// keeps working exactly as before. The control limits are set once, at inspection-creation
/// time (see <see cref="Inspection.Create(Guid, InspectionType, Guid, IReadOnlyCollection{InspectionCharacteristicInput})"/>)
/// — there is no per-characteristic-spec master record in this slice (see
/// GetSpcDataForCharacteristicQuery's doc comment for that documented scope-out), so the
/// limits live directly on the item they were set for.
/// </summary>
public sealed class InspectionItem
{
    public Guid Id { get; private set; }
    public string Characteristic { get; private set; } = default!;
    public bool? Passed { get; private set; }
    public string? Notes { get; private set; }
    public decimal? MeasuredValue { get; private set; }
    public decimal? LowerControlLimit { get; private set; }
    public decimal? UpperControlLimit { get; private set; }

    private InspectionItem() { }

    internal static InspectionItem Create(string characteristic, decimal? lowerControlLimit = null, decimal? upperControlLimit = null)
    {
        if (string.IsNullOrWhiteSpace(characteristic))
            throw new ArgumentException("Inspection characteristic is required.", nameof(characteristic));
        if (lowerControlLimit.HasValue && upperControlLimit.HasValue && lowerControlLimit.Value > upperControlLimit.Value)
            throw new ArgumentException($"Characteristic '{characteristic}': lower control limit cannot exceed the upper control limit.", nameof(lowerControlLimit));

        return new InspectionItem
        {
            Id = Guid.NewGuid(),
            Characteristic = characteristic.Trim(),
            LowerControlLimit = lowerControlLimit,
            UpperControlLimit = upperControlLimit,
        };
    }

    /// <summary>
    /// Records this characteristic's result. <paramref name="passed"/> is an explicit
    /// pass/fail override; <paramref name="measuredValue"/> is the optional numeric SPC
    /// reading. Resolution rule (documented here since it's a judgment call, not something
    /// obvious from the signature): an explicit <paramref name="passed"/> always wins — if
    /// the caller supplies one, it is used as-is, even if a measured value and both control
    /// limits are also present. Only when <paramref name="passed"/> is omitted (null) is
    /// Passed auto-derived from <paramref name="measuredValue"/> falling within
    /// [<see cref="LowerControlLimit"/>, <see cref="UpperControlLimit"/>] (inclusive). If
    /// neither an explicit result nor a derivable measured value/limit set is available,
    /// this throws — every characteristic must resolve to a real pass/fail.
    /// </summary>
    internal void RecordResult(bool? passed, decimal? measuredValue, string? notes)
    {
        bool resolvedPassed;
        if (passed.HasValue)
        {
            resolvedPassed = passed.Value;
        }
        else if (measuredValue.HasValue && LowerControlLimit.HasValue && UpperControlLimit.HasValue)
        {
            resolvedPassed = measuredValue.Value >= LowerControlLimit.Value && measuredValue.Value <= UpperControlLimit.Value;
        }
        else
        {
            throw new ArgumentException(
                $"Characteristic '{Characteristic}' requires either an explicit Passed result, or a MeasuredValue together with both control limits set on the characteristic so a result can be auto-derived.",
                nameof(passed));
        }

        Passed = resolvedPassed;
        MeasuredValue = measuredValue;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
