using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FusionOS.BuildingBlocks.Infrastructure.Security;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FusionOS.Modules.Core.Infrastructure.Security;

/// <summary>
/// Issues access tokens as standard signed JWTs (HMAC-SHA256) carrying the same
/// sub/company_id/branch_id/permissions claims HttpCurrentUserContext already
/// reads (see ICurrentUserContext). Refresh tokens are opaque random values -
/// only their SHA-256 hash is ever persisted, per 07_SECURITY.md.
///
/// Permissions are packed into ONE gzip+base64url claim rather than one JWT
/// claim per permission code. A role holding every catalog permission (e.g. the
/// default Owner role, or any company whose permission catalog has grown since
/// the role was last synced) previously produced an Authorization header north
/// of 16KB once the catalog passed a few hundred codes - large enough to trip
/// the default request-header-size limit on Node/Vite's dev server (HTTP 431)
/// and to sit uncomfortably close to common reverse-proxy header-buffer limits
/// (nginx's default 8KB, for one) in front of a real deployment. Gzip exploits
/// the heavy shared-prefix redundancy in dot-delimited permission codes
/// ("finance.account.create", "finance.account.read", ...) to keep this small
/// even as the catalog keeps growing - see JwtPermissionsCodec for the exact
/// encode/decode pair HttpCurrentUserContext uses on the other end.
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
        var encodedPermissions = JwtPermissionsCodec.Encode(claims.Permissions);
        if (encodedPermissions.Length > 0)
            claimsList.Add(new Claim("permissions", encodedPermissions));

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
