namespace FusionOS.Modules.Quality.Domain.Inspections;

/// <summary>
/// One characteristic checked as part of an <see cref="Inspection"/> (e.g. "Dimensional
/// tolerance", "Surface finish"). <see cref="Passed"/> is null until results are recorded.
/// Same documented no-audit/tenant-columns exception as every other line entity — its
/// lifecycle is owned entirely by the parent Inspection.
/// </summary>
public sealed class InspectionItem
{
    public Guid Id { get; private set; }
    public string Characteristic { get; private set; } = default!;
    public bool? Passed { get; private set; }
    public string? Notes { get; private set; }

    private InspectionItem() { }

    internal static InspectionItem Create(string characteristic)
    {
        if (string.IsNullOrWhiteSpace(characteristic))
            throw new ArgumentException("Inspection characteristic is required.", nameof(characteristic));

        return new InspectionItem
        {
            Id = Guid.NewGuid(),
            Characteristic = characteristic.Trim(),
        };
    }

    internal void RecordResult(bool passed, string? notes)
    {
        Passed = passed;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
