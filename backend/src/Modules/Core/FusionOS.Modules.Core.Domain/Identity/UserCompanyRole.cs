namespace FusionOS.Modules.Core.Domain.Identity;

/// <summary>Join entity giving a User a Role within a specific Company (and optionally Branch) — 04_DATABASE_GUIDELINES.md §6.</summary>
public sealed class UserCompanyRole
{
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid RoleId { get; private set; }

    private UserCompanyRole() { }

    public static UserCompanyRole Create(Guid userId, Guid companyId, Guid roleId, Guid? branchId = null) => new()
    {
        UserId = userId,
        CompanyId = companyId,
        BranchId = branchId,
        RoleId = roleId,
    };
}
