namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Marks a command/query as DELIBERATELY exempt from AuthorizationBehavior's
/// permission check (Phase 9 hardening fix, 2026-07-25). Before this existed,
/// AuthorizationBehavior silently allowed ANY request through unchecked simply
/// by not implementing IRequirePermission — a fail-OPEN default: a future
/// command that forgot to add IRequirePermission would create a silent,
/// untested RBAC hole with no compiler or runtime signal at all.
///
/// AuthorizationBehavior now REQUIRES every command/query to implement either
/// IRequirePermission or this interface — there is no third, unmarked option.
/// Genuinely public actions (pre-auth bootstrap: CreateCompanyCommand,
/// RegisterUserCommand, LoginCommand's own request type if it's a command,
/// RefreshTokenCommand, LogoutCommand; or actions gated by a DIFFERENT,
/// non-RBAC authorization mechanism: customer-portal commands like
/// RejectOwnQuotationCommand, which derive the caller's identity from a
/// separate portal-JWT claim rather than a staff permission) implement this
/// instead, making the "no permission check here" decision an explicit,
/// reviewable choice at the command's own declaration site rather than an
/// easily-forgotten omission.
/// </summary>
public interface IPubliclyAccessible
{
}
