using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Organizations;

public sealed class Department : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public Guid? ParentDepartmentId { get; private set; }

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
}
