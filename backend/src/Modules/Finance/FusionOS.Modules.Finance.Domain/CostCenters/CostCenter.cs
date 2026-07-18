using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.CostCenters.Events;

namespace FusionOS.Modules.Finance.Domain.CostCenters;

/// <summary>
/// M8a — Finance depth: Cost Centers. Pure master data (Code/Name/IsActive),
/// deliberately no hierarchy (unlike Account's self-referencing ParentAccountId) —
/// the PRD line only asks for "cost centers," not a cost-center tree; adding one
/// unrequested would be new scope, not the requested slice. A CostCenter is not
/// yet attached to JournalEntryLine — this slice only builds the master-data
/// aggregate itself; wiring an optional CostCenterId onto journal lines is a
/// natural, separately-scoped follow-up once this exists to reference.
/// </summary>
public sealed class CostCenter : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private CostCenter() { }

    public static CostCenter Create(Guid companyId, string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Cost center code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cost center name is required.", nameof(name));

        var costCenter = new CostCenter
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
        };

        costCenter.Raise(new CostCenterCreated(costCenter.Id, companyId, costCenter.Code));
        return costCenter;
    }

    /// <summary>Updates the mutable master-data field. Code and CompanyId are the tenant-scoped business key and stay immutable after creation.</summary>
    public void UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cost center name is required.", nameof(name));

        Name = name.Trim();
    }

    public void Deactivate() => IsActive = false;
}
