namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Marks a command whose CompanyId is intentionally NOT checked against
/// ICurrentUserContext.CompanyId by TenantIsolationBehavior (Phase 9 hardening
/// fix, 2026-07-25). TenantIsolationBehavior reflects on any "CompanyId" Guid
/// property and rejects the request unless it matches the caller's own JWT
/// company_id claim — correct for almost every command, but wrong for a
/// command reachable by an ANONYMOUS caller (no claim exists yet to check
/// against), which the reflection-based rule would reject unconditionally
/// regardless of the controller's own [AllowAnonymous].
///
/// RegisterUserCommand is the confirmed case this closes: AuthController.Register
/// is deliberately [AllowAnonymous] so a brand-new company's first user can
/// bootstrap without being logged in, and RegisterUserCommandHandler already
/// enforces its own correct, data-dependent rule internally
/// (CompanyHasAnyUsersAsync + core.user.register for every registration after
/// the first) — TenantIsolationBehavior's blanket CompanyId check was firing
/// BEFORE that handler logic ever ran, making the endpoint 403 for every
/// anonymous caller and silently breaking new-tenant bootstrap entirely (found
/// via Phase 9's integration-test-writing pass, not previously caught since no
/// .NET SDK/Docker in this environment has ever let these tests actually run).
///
/// Use sparingly — this bypasses TenantIsolationBehavior's central chokepoint,
/// so only mark a command with this when its handler performs an equivalent
/// (or stricter) check itself.
/// </summary>
public interface IBypassesTenantIsolation
{
}
