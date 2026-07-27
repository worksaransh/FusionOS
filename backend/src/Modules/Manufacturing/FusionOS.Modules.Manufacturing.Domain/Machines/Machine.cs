using FusionOS.SharedKernel;

namespace FusionOS.Modules.Manufacturing.Domain.Machines;

/// <summary>
/// Machine Scheduling support (Phase 6, 2026-07-22) — the piece of equipment a
/// WorkOrderOperation is scheduled onto. WorkCenterId is a same-module reference to
/// WorkCenter (validated by the command handler before creation, same convention as
/// WorkOrder.BillOfMaterialsId), since a machine belongs to exactly one work center.
/// </summary>
public sealed class Machine : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Guid WorkCenterId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Machine() { }

    public static Machine Create(Guid companyId, string code, string name, Guid workCenterId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Machine code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Machine name is required.", nameof(name));
        if (workCenterId == Guid.Empty)
            throw new ArgumentException("Work center id is required.", nameof(workCenterId));

        return new Machine
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            WorkCenterId = workCenterId,
        };
    }

    /// <summary>The standard soft-deactivate — never a hard delete.</summary>
    public void Deactivate() => IsActive = false;
}
