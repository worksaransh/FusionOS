using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Enforces RBAC permission checks server-side — the actual security boundary
/// per 07_SECURITY.md §2. The frontend permission-aware UI (06_UI_UX_DESIGN_SYSTEM.md
/// §8) is a UX layer only; this behavior is what actually protects the data.
///
/// FAILS CLOSED (Phase 9 hardening fix, 2026-07-25): every command/query MUST
/// implement either IRequirePermission or IPubliclyAccessible — there is no
/// unmarked third option. Before this, a request implementing neither simply
/// passed through with zero check (a fail-OPEN default), meaning a future
/// command that forgot IRequirePermission would create a silent RBAC hole with
/// no error, no test failure, nothing. A grep-based audit of all ~538
/// commands/queries in this codebase was run TWICE (2026-07-25, then again
/// 2026-07-27 as part of a full Phase 10 enterprise audit) and under-counted
/// BOTH times by sampling file text rather than diffing exact declaration
/// statements — the second, exhaustive diff-based pass finally found the true,
/// complete set of 15 legitimate exceptions, confirmed by an empty set-difference
/// between "every ICommand/IQuery declaration" and "every declaration
/// implementing IRequirePermission or IPubliclyAccessible": CreateCompanyCommand,
/// RegisterUserCommand, LoginCommand, RefreshTokenCommand, LogoutCommand
/// (pre-auth bootstrap); DeleteCommentCommand (author-or-permission-holder,
/// checked in-handler); IngestShopifyOrderWebhookCommand (HMAC-gated, not RBAC);
/// PortalLoginCommand (pre-auth portal login); and the customer-portal "my
/// records" surface — RejectOwnQuotationCommand, AcceptOwnQuotationCommand,
/// ListMyDispatchesQuery, ListMyInvoicesQuery, ListMySalesOrdersQuery,
/// ListMyQuotationsQuery — gated by a separate portal-JWT/policy instead of
/// staff RBAC. All 15 are now explicitly marked IPubliclyAccessible rather than
/// just quietly unmarked. LESSON: never trust a file-text grep for this kind of
/// audit again — always diff exact declaration lines against the marker set.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserContext _currentUser;

    public AuthorizationBehavior(ICurrentUserContext currentUser) => _currentUser = currentUser;

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequirePermission requirement)
        {
            foreach (var permission in requirement.RequiredPermissions)
            {
                if (!_currentUser.HasPermission(permission))
                    throw new ForbiddenException(permission);
            }
        }
        else if (request is not IPubliclyAccessible)
        {
            // Fail closed: a command/query implementing neither interface is a
            // programming error (a forgotten IRequirePermission), not a
            // legitimate public action — never silently allow it through.
            throw new ForbiddenException($"unclassified-request:{typeof(TRequest).Name}");
        }

        return next();
    }
}
