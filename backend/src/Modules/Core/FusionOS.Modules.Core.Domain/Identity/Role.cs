using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Identity;

/// <summary>System roles have CompanyId == null (platform-defined); company-custom roles set it.</summary>
public sealed class Role : AuditableEntity
{
    public Guid? CompanyId { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsSystemRole { get; private set; }

    private Role() { }

    public static Role CreateSystemRole(string name) => new() { Name = name, IsSystemRole = true };

    public static Role CreateCompanyRole(Guid companyId, string name) =>
        new() { CompanyId = companyId, Name = name, IsSystemRole = false };
}
