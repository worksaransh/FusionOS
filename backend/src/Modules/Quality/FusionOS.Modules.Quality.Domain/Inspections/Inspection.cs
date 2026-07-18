using FusionOS.SharedKernel;
using FusionOS.Modules.Quality.Domain.Inspections.Events;

namespace FusionOS.Modules.Quality.Domain.Inspections;

/// <summary>
/// Phase 5 — Quality, first slice. A quality inspection of something produced or received:
/// a set of characteristics to check, each recorded pass/fail, resolving the whole
/// inspection to Passed (all characteristics passed) or Failed (any failed).
///
/// <see cref="ReferenceId"/> is an opaque cross-module reference — a Manufacturing
/// WorkOrder id for a <see cref="InspectionType.Production"/> inspection, or a
/// Procurement/Warehouse Goods Receipt id for <see cref="InspectionType.IncomingGoods"/>
/// — never existence-validated here, same convention as InventoryLedgerEntry's WarehouseId.
/// The type discriminator plus the id is what ties this back to those modules; no
/// cross-module read or foreign key is taken.
/// </summary>
public sealed class Inspection : TenantAggregateRoot
{
    private readonly List<InspectionItem> _items = new();

    public InspectionType Type { get; private set; }
    public Guid ReferenceId { get; private set; }
    public InspectionStatus Status { get; private set; }
    public IReadOnlyList<InspectionItem> Items => _items.AsReadOnly();

    private Inspection() { }

    public static Inspection Create(Guid companyId, InspectionType type, Guid referenceId, IReadOnlyCollection<string> characteristics)
    {
        if (referenceId == Guid.Empty)
            throw new ArgumentException("A reference id (the work order or goods receipt being inspected) is required.", nameof(referenceId));
        if (characteristics is null || characteristics.Count == 0)
            throw new ArgumentException("An inspection must check at least one characteristic.", nameof(characteristics));

        var inspection = new Inspection
        {
            CompanyId = companyId,
            Type = type,
            ReferenceId = referenceId,
            Status = InspectionStatus.Pending,
        };

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var characteristic in characteristics)
        {
            var trimmed = characteristic?.Trim() ?? string.Empty;
            if (trimmed.Length == 0)
                throw new ArgumentException("An inspection characteristic cannot be blank.", nameof(characteristics));
            if (!seen.Add(trimmed))
                throw new ArgumentException($"Characteristic '{trimmed}' appears more than once in the inspection.", nameof(characteristics));

            inspection._items.Add(InspectionItem.Create(trimmed));
        }

        inspection.Raise(new InspectionCreated(inspection.Id, companyId, type.ToString(), referenceId));
        return inspection;
    }

    /// <summary>
    /// Records a pass/fail for every characteristic and resolves the inspection. Every
    /// characteristic must be given exactly one result and every result must match a
    /// characteristic on this inspection — a partial submission is rejected rather than
    /// silently leaving items unresolved. Overall Status is Passed only if every item
    /// passed, else Failed. Raises <see cref="InspectionCompleted"/>.
    /// </summary>
    public void RecordResults(IReadOnlyCollection<InspectionResultInput> results)
    {
        if (Status != InspectionStatus.Pending)
            throw new InvalidOperationException($"Only a Pending inspection can have results recorded (current status: {Status}).");
        if (results is null || results.Count == 0)
            throw new ArgumentException("At least one result is required.", nameof(results));

        var byCharacteristic = new Dictionary<string, InspectionResultInput>(StringComparer.OrdinalIgnoreCase);
        foreach (var result in results)
        {
            if (!byCharacteristic.TryAdd(result.Characteristic?.Trim() ?? string.Empty, result))
                throw new ArgumentException($"Characteristic '{result.Characteristic}' has more than one result.", nameof(results));
        }

        foreach (var item in _items)
        {
            if (!byCharacteristic.TryGetValue(item.Characteristic, out var result))
                throw new ArgumentException($"No result was provided for characteristic '{item.Characteristic}'.", nameof(results));
            item.RecordResult(result.Passed, result.Notes);
        }

        if (byCharacteristic.Count != _items.Count)
            throw new ArgumentException("A result was provided for a characteristic that is not part of this inspection.", nameof(results));

        Status = _items.All(i => i.Passed == true) ? InspectionStatus.Passed : InspectionStatus.Failed;
        Raise(new InspectionCompleted(Id, CompanyId, Type.ToString(), ReferenceId, Status == InspectionStatus.Passed));
    }
}
