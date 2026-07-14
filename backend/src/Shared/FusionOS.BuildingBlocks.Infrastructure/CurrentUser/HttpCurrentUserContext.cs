using System.Security.Claims;
using FusionOS.SharedKernel.Context;
using Microsoft.AspNetCore.Http;

namespace FusionOS.BuildingBlocks.Infrastructure.CurrentUser;

/// <summary>
/// Resolves the ambient user/tenant context from the current HTTP request's JWT
/// claims (07_SECURITY.md §1-§2). Background jobs/consumers use a different
/// implementation that carries context explicitly instead of via HttpContext.
/// </summary>
public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUserContext(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public Guid? UserId => Guid.TryParse(User?.FindFirstValue("sub"), out var id) ? id : null;
    public Guid? CompanyId => Guid.TryParse(User?.FindFirstValue("company_id"), out var id) ? id : null;
    public Guid? BranchId => Guid.TryParse(User?.FindFirstValue("branch_id"), out var id) ? id : null;

    public IReadOnlyCollection<string> Permissions =>
        User?.FindAll("permission").Select(c => c.Value).ToArray() ?? Array.Empty<string>();

    public bool HasPermission(string permissionCode) => Permissions.Contains(permissionCode);

    public string CorrelationId => _accessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
}
