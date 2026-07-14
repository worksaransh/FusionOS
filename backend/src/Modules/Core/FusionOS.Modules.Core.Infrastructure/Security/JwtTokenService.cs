using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FusionOS.Modules.Core.Infrastructure.Security;

/// <summary>
/// Issues access tokens as standard signed JWTs (HMAC-SHA256) carrying the same
/// sub/company_id/branch_id/permission claims HttpCurrentUserContext already
/// reads (see ICurrentUserContext). Refresh tokens are opaque random values -
/// only their SHA-256 hash is ever persisted, per 07_SECURITY.md.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options) => _options = options.Value;

    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(_options.RefreshTokenLifetimeDays);

    public AccessToken GenerateAccessToken(TokenClaims claims)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);

        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, claims.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, claims.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (claims.CompanyId is { } companyId)
            claimsList.Add(new Claim("company_id", companyId.ToString()));
        if (claims.BranchId is { } branchId)
            claimsList.Add(new Claim("branch_id", branchId.ToString()));
        claimsList.AddRange(claims.Permissions.Select(p => new Claim("permission", p)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claimsList,
            expires: expiresAt.UtcDateTime,
            signingCredentials: signingCredentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public string HashRefreshTokenValue(string rawValue)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawValue));
        return Convert.ToHexString(bytes);
    }
}
