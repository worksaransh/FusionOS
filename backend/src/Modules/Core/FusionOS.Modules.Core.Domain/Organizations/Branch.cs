using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Organizations;

public sealed class Branch : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsHeadOffice { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Branch() { }

    public static Branch Create(Guid companyId, string name, string code, bool isHeadOffice = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Branch name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Branch code is required.", nameof(code));

        return new Branch
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            IsHeadOffice = isHeadOffice,
        };
    }

    /// <summary>Updates the mutable master-data fields. Code is the tenant-scoped business key and stays immutable after creation, same convention as CostCenter/Company.</summary>
    public void UpdateDetails(string name, bool isHeadOffice)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Branch name is required.", nameof(name));

        Name = name.Trim();
        IsHeadOffice = isHeadOffice;
    }

    public void Deactivate() => IsActive = false;
}
