using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Organizations;

public sealed class Department : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public Guid? ParentDepartmentId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Department() { }

    public static Department Create(Guid companyId, Guid? branchId, string name, string code, Guid? parentDepartmentId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Department name is required.", nameof(name));

        return new Department
        {
            CompanyId = companyId,
            BranchId = branchId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            ParentDepartmentId = parentDepartmentId,
        };
    }

    /// <summary>
    /// Updates the mutable master-data fields. Code is the tenant-scoped
    /// business key and stays immutable after creation, same convention as
    /// Branch/CostCenter/Company. Unlike Branch, Department's Code is not
    /// enforced unique per company (see DepartmentConfiguration's non-unique
    /// index) — a department code may legitimately repeat across branches.
    /// </summary>
    public void UpdateDetails(string name, Guid? branchId, Guid? parentDepartmentId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Department name is required.", nameof(name));

        Name = name.Trim();
        BranchId = branchId;
        ParentDepartmentId = parentDepartmentId;
    }

    public void Deactivate() => IsActive = false;
}
