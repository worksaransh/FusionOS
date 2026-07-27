namespace FusionOS.Modules.Sales.Application.PortalUsers.Contracts;

/// <summary>Claims a portal access token carries - see PortalJwtTokenService (Infrastructure) for how these become JWT claims.</summary>
public sealed record PortalTokenClaims(Guid PortalUserId, Guid CustomerId, string Email);

public sealed record PortalAccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>
/// Sales-owned mirror of Core.Application.Auth.Contracts.IJwtTokenService,
/// narrowed to only what the portal needs (no refresh tokens in this minimal
/// slice - see PortalLoginCommandHandler's doc comment). Implemented in
/// Sales.Infrastructure using the same System.IdentityModel.Tokens.Jwt package
/// and the SAME "Jwt" configuration section (signing key/issuer/audience) as
/// Core's own JwtTokenService, so the one ASP.NET Core JWT bearer scheme
/// registered in Program.cs can validate both staff and customer tokens.
/// Every token this issues carries a "token_type": "customer" claim plus
/// "customer_id" and deliberately NO "permission" claims, which is what keeps
/// a customer token from ever satisfying a staff endpoint's IRequirePermission
/// check (AuthorizationBehavior only ever grants what HttpCurrentUserContext
/// reads off "permission" claims).
/// </summary>
public interface IPortalTokenService
{
    PortalAccessToken GenerateAccessToken(PortalTokenClaims claims);
}
