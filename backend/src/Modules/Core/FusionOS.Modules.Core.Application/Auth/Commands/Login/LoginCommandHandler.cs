using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FluentValidation.Results;
using MediatR;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Deliberately the SAME error for "no such user" and "wrong password" -
        // a distinct message would let an attacker enumerate registered emails.
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new ValidationException(new[] { new ValidationFailure("email", "Invalid email or password.") });

        var companyRoles = await _users.GetCompanyRolesAsync(user.Id, cancellationToken);

        Guid? companyId = null;
        Guid? branchId = null;
        var permissions = Array.Empty<string>();

        if (companyRoles.Count > 0)
        {
            var selected = request.CompanyId is { } wanted
                ? companyRoles.FirstOrDefault(cr => cr.CompanyId == wanted)
                : companyRoles.First();

            if (selected == default)
                throw new ValidationException(new[] { new ValidationFailure("companyId", "You do not have access to that company.") });

            companyId = selected.CompanyId;
            branchId = selected.BranchId;
            permissions = (await _users.GetRolePermissionCodesAsync(selected.RoleId, cancellationToken)).ToArray();
        }

        var accessToken = _tokenService.GenerateAccessToken(new TokenClaims(user.Id, user.Email, companyId, branchId, permissions));

        var rawRefreshToken = _tokenService.GenerateRefreshTokenValue();
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.Add(_tokenService.RefreshTokenLifetime);
        var refreshTokenEntity = Domain.Identity.RefreshToken.Issue(
            user.Id, companyId, branchId, _tokenService.HashRefreshTokenValue(rawRefreshToken), refreshTokenExpiresAt);

        await _refreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(
            user.Id, user.Email, user.FullName, companyId, branchId, permissions,
            accessToken.Value, accessToken.ExpiresAt, rawRefreshToken, refreshTokenExpiresAt);
    }
}
