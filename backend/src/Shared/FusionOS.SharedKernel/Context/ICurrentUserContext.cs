namespace FusionOS.SharedKernel.Context;

/// <summary>
/// Ambient request context: who is calling, in which company/branch, with which
/// permissions. Implemented in BuildingBlocks.Infrastructure via IHttpContextAccessor.
/// </summary>
public interface ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? CompanyId { get; }
    Guid? BranchId { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool HasPermission(string permissionCode);
    string CorrelationId { get; }
}
