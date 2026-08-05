using FusionOS.SharedKernel;

namespace FusionOS.Modules.Manufacturing.Domain.WorkCenters;

/// <summary>
/// Capacity Planning support (Phase 6, 2026-07-22) — the master-data aggregate
/// RoutingOperation's own doc comment flagged as "not yet a real need for this first
/// slice." RoutingOperation.WorkCenter stays a free-text field (changing it to a real
/// FK would be a breaking change to existing BOMs) — this is purely additive: a
/// WorkCenter has a CapacityHoursPerDay that GetWorkCenterCapacityLoadQuery compares
/// planned routing-operation load against. Matching a RoutingOperation's free-text
/// WorkCenter name to a real WorkCenter's Code/Name is the query handler's job (string
/// match), not a hard FK, since existing routing operations predate this entity.
/// </summary>
public sealed class WorkCenter : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public decimal CapacityHoursPerDay { get; private set; }
    public bool IsActive { get; private set; } = true;

    private WorkCenter() { }

    public static WorkCenter Create(Guid companyId, string code, string name, decimal capacityHoursPerDay)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Work center code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Work center name is required.", nameof(name));
        if (capacityHoursPerDay <= 0)
            throw new ArgumentException("Capacity hours per day must be greater than zero.", nameof(capacityHoursPerDay));

        return new WorkCenter
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CapacityHoursPerDay = capacityHoursPerDay,
        };
    }

    public void UpdateCapacity(decimal capacityHoursPerDay)
    {
        if (capacityHoursPerDay <= 0)
            throw new ArgumentException("Capacity hours per day must be greater than zero.", nameof(capacityHoursPerDay));

        CapacityHoursPerDay = capacityHoursPerDay;
    }

    /// <summary>Updates the editable header fields. Code is deliberately excluded — it's the immutable business key, same convention as UpdateBranchCommand/UpdateCostCenterCommand.</summary>
    public void UpdateDetails(string name, decimal capacityHoursPerDay)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Work center name is required.", nameof(name));
        if (capacityHoursPerDay <= 0)
            throw new ArgumentException("Capacity hours per day must be greater than zero.", nameof(capacityHoursPerDay));

        Name = name.Trim();
        CapacityHoursPerDay = capacityHoursPerDay;
    }

    /// <summary>The standard soft-deactivate — never a hard delete.</summary>
    public void Deactivate() => IsActive = false;
}
