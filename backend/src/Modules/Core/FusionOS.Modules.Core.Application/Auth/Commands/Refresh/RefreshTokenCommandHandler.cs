using FluentValidation.Results;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashRefreshTokenValue(request.RefreshToken);
        var existing = await _refreshTokens.GetActiveByHashAsync(hash, cancellationToken);
        if (existing is null)
            throw new ValidationException(new[] { new ValidationFailure("refreshToken", "That refresh token is invalid, expired, or already used.") });

        var user = await _users.GetByIdAsync(existing.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new ValidationException(new[] { new ValidationFailure("refreshToken", "That refresh token is invalid, expired, or already used.") });

        // Re-derive current permissions rather than trusting the ones baked into
        // the previous access token — a revoked role must take effect immediately.
        var permissions = Array.Empty<string>();
        if (existing.CompanyId is { } companyId)
        {
            var companyRoles = await _users.GetCompanyRolesAsync(user.Id, cancellationToken);
            var match = companyRoles.FirstOrDefault(cr => cr.CompanyId == companyId);
            if (match != default)
                permissions = (await _users.GetRolePermissionCodesAsync(match.RoleId, cancellationToken)).ToArray();
        }

        var accessToken = _tokenService.GenerateAccessToken(
            new TokenClaims(user.Id, user.Email, existing.CompanyId, existing.BranchId, permissions));

        var rawNewRefreshToken = _tokenService.GenerateRefreshTokenValue();
        var newExpiresAt = DateTimeOffset.UtcNow.Add(_tokenService.RefreshTokenLifetime);
        var newTokenEntity = Domain.Identity.RefreshToken.Issue(
            user.Id, existing.CompanyId, existing.BranchId, _tokenService.HashRefreshTokenValue(rawNewRefreshToken), newExpiresAt);

        existing.Revoke(newTokenEntity.Id);
        await _refreshTokens.AddAsync(newTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(
            user.Id, user.Email, user.FullName, existing.CompanyId, existing.BranchId, permissions,
            accessToken.Value, accessToken.ExpiresAt, rawNewRefreshToken, newExpiresAt);
    }
}
