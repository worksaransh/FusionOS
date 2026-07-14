namespace FusionOS.Modules.Core.Application.Auth.Contracts;

public sealed record AuthResultDto(
    Guid UserId,
    string Email,
    string FullName,
    Guid? CompanyId,
    Guid? BranchId,
    IReadOnlyCollection<string> Permissions,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

public sealed record UserDto(Guid Id, string Email, string FullName, bool IsActive, DateTimeOffset CreatedAt);
