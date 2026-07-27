namespace FusionOS.Modules.Quality.Domain.InspectionPlans;

/// <summary>
/// One characteristic name on an <see cref="InspectionPlan"/> template (e.g. "Dimensional
/// tolerance"). Same "owned by parent, no own audit/tenant columns" convention as
/// <see cref="Inspections.InspectionItem"/> on Inspection — its lifecycle is owned entirely
/// by the parent InspectionPlan. Unlike InspectionItem, it never records a pass/fail result
/// itself; it is only ever copied into a new Inspection's characteristic list.
/// </summary>
public sealed class InspectionPlanCharacteristic
{
    public Guid Id { get; private set; }
    public string CharacteristicName { get; private set; } = default!;
    public int SequenceNumber { get; private set; }

    private InspectionPlanCharacteristic() { }

    internal static InspectionPlanCharacteristic Create(string characteristicName, int sequenceNumber)
    {
        if (string.IsNullOrWhiteSpace(characteristicName))
            throw new ArgumentException("Characteristic name is required.", nameof(characteristicName));

        return new InspectionPlanCharacteristic
        {
            Id = Guid.NewGuid(),
            CharacteristicName = characteristicName.Trim(),
            SequenceNumber = sequenceNumber,
        };
    }
}
