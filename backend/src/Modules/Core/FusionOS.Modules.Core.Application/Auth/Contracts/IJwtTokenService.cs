namespace FusionOS.Modules.Core.Application.Auth.Contracts;

public sealed record TokenClaims(Guid UserId, string Email, Guid? CompanyId, Guid? BranchId, IReadOnlyCollection<string> Permissions);

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>Issues and hashes tokens. Concrete implementation (System.IdentityModel.Tokens.Jwt) lives in Infrastructure.</summary>
public interface IJwtTokenService
{
    AccessToken GenerateAccessToken(TokenClaims claims);

    /// <summary>Returns the raw, opaque refresh-token value handed to the client. Never persisted in plain text.</summary>
    string GenerateRefreshTokenValue();

    /// <summary>One-way hash of a raw refresh-token value, used both to persist and to look one up.</summary>
    string HashRefreshTokenValue(string rawValue);

    TimeSpan RefreshTokenLifetime { get; }
}
