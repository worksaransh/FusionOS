using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FusionOS.Modules.Sales.Application.PortalUsers.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FusionOS.Modules.Sales.Infrastructure.Security;

/// <summary>
/// Issues customer portal access tokens as standard signed JWTs (HMAC-SHA256),
/// signed with the same key/issuer/audience as Core.Infrastructure.Security.JwtTokenService
/// (see PortalJwtOptions) so the one JWT bearer scheme in Program.cs validates
/// both. Every token carries "token_type": "customer" and "customer_id" and
/// deliberately NO "permission" claims - see PortalLoginCommandHandler's doc
/// comment for why that absence is what keeps a customer token from ever
/// satisfying a staff endpoint's IRequirePermission check.
/// </summary>
public sealed class PortalJwtTokenService : IPortalTokenService
{
    private readonly PortalJwtOptions _options;

    public PortalJwtTokenService(IOptions<PortalJwtOptions> options) => _options = options.Value;

    public PortalAccessToken GenerateAccessToken(PortalTokenClaims claims)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.PortalAccessTokenLifetimeMinutes);

        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, claims.PortalUserId.ToString()),
            new(JwtRegisteredClaimNames.Email, claims.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("customer_id", claims.CustomerId.ToString()),
            new("token_type", "customer"),
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claimsList,
            expires: expiresAt.UtcDateTime,
            signingCredentials: signingCredentials);

        return new PortalAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
