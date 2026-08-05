using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Enforces that the caller's company is actually entitled — per its Billing Subscription/Plan
/// — to the module a command/query belongs to. Runs immediately after AuthorizationBehavior in
/// the pipeline (registered right after it in AddModuleApplication below): AuthorizationBehavior
/// already establishes "this user is allowed to do this within their company"; this behavior
/// answers the separate question "is this company's plan allowed to do this at all."
///
/// Module derivation: every permission code in this codebase is "{module}.{resource}.{action}"
/// (see PermissionCatalog.cs — "core.user.register", "manufacturing.work-order.create", etc.),
/// so the module name is exactly the substring before the first '.'. This behavior derives the
/// target module from the FIRST entry in IRequirePermission.RequiredPermissions — a request's
/// permissions are always drawn from a single module in this codebase, so the first is
/// representative of all of them.
///
/// Skips entirely (falls straight through to next()) when:
///  - the request doesn't implement IRequirePermission at all (nothing to check against a
///    module — e.g. Login/Register/RefreshToken, already exempted from AuthorizationBehavior's
///    own check via IPubliclyAccessible), or
///  - RequiredPermissions is empty, or
///  - every required permission starts with "core." — Core must always be reachable regardless
///    of plan, so a company can never lock itself out of managing its own subscription/users/
///    roles by downgrading or lapsing.
///
/// See IPlanEntitlementService's doc comment for why the underlying check fails OPEN (allows
/// access) whenever no Subscription exists or Billing's data can't be reached — that is the
/// deliberate backward-compatibility default for this behavior, not a bug.
/// </summary>
public sealed class PlanEntitlementBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IPlanEntitlementService _planEntitlementService;

    public PlanEntitlementBehavior(ICurrentUserContext currentUser, IPlanEntitlementService planEntitlementService)
    {
        _currentUser = currentUser;
        _planEntitlementService = planEntitlementService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequirePermission requirement && requirement.RequiredPermissions.Length > 0)
        {
            var moduleName = ModuleNameOf(requirement.RequiredPermissions[0]);

            // Core is always allowed, regardless of plan — see class doc comment.
            if (!string.Equals(moduleName, "core", StringComparison.OrdinalIgnoreCase))
            {
                // No ambient company (e.g. a permission-marked request somehow reachable
                // pre-auth) — nothing to look a Subscription up against; nothing to block.
                if (_currentUser.CompanyId is { } companyId)
                {
                    var included = await _planEntitlementService.IsModuleIncludedAsync(companyId, moduleName, cancellationToken);
                    if (!included)
                    {
                        throw ForbiddenException.WithMessage(
                            $"Your current plan does not include the '{moduleName}' module. Upgrade to access this feature.");
                    }
                }
            }
        }

        return await next();
    }

    private static string ModuleNameOf(string permissionCode)
    {
        var dotIndex = permissionCode.IndexOf('.');
        return dotIndex > 0 ? permissionCode[..dotIndex] : permissionCode;
    }
}
