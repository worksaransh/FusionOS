using FusionOS.SharedKernel;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Domain.InspectionPlans;

/// <summary>
/// Phase 5 — Quality: a reusable checklist template for a given <see cref="InspectionType"/>
/// — e.g. a standing "Incoming Goods — Fasteners" plan with its usual set of characteristics,
/// so a caller opening that kind of inspection over and over doesn't have to hand-type the
/// same characteristic list every time. Purely a convenience default: <see cref="Inspection"/>
/// itself still owns its own freeform Items list and has no hard dependency on any plan
/// (see CreateInspectionCommand's optional InspectionPlanId).
///
/// Owns an ordered collection of <see cref="InspectionPlanCharacteristic"/>, same
/// "owned by parent, no own audit/tenant columns" convention as Inspection's own Items.
/// </summary>
public sealed class InspectionPlan : TenantAggregateRoot
{
    private readonly List<InspectionPlanCharacteristic> _characteristics = new();

    public string Name { get; private set; } = default!;
    public InspectionType InspectionType { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<InspectionPlanCharacteristic> Characteristics => _characteristics.AsReadOnly();

    private InspectionPlan() { }

    public static InspectionPlan Create(Guid companyId, string name, InspectionType inspectionType, IReadOnlyCollection<string> characteristicNames)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Inspection plan name is required.", nameof(name));
        if (characteristicNames is null || characteristicNames.Count == 0)
            throw new ArgumentException("An inspection plan must define at least one characteristic.", nameof(characteristicNames));

        var plan = new InspectionPlan
        {
            CompanyId = companyId,
            Name = name.Trim(),
            InspectionType = inspectionType,
            IsActive = true,
        };

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sequence = 1;
        foreach (var characteristicName in characteristicNames)
        {
            var trimmed = characteristicName?.Trim() ?? string.Empty;
            if (trimmed.Length == 0)
                throw new ArgumentException("An inspection plan characteristic cannot be blank.", nameof(characteristicNames));
            if (!seen.Add(trimmed))
                throw new ArgumentException($"Characteristic '{trimmed}' appears more than once in the plan.", nameof(characteristicNames));

            plan._characteristics.Add(InspectionPlanCharacteristic.Create(trimmed, sequence));
            sequence++;
        }

        return plan;
    }

    /// <summary>Retires the plan — it stays queryable (and still linkable from past inspections) but should no longer be offered as a default when creating new inspections.</summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("This inspection plan is already inactive.");

        IsActive = false;
    }
}
