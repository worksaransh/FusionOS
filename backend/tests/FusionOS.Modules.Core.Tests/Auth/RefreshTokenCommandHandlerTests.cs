using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Commands.Refresh;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private static (IUserRepository users, IRefreshTokenRepository refreshTokens, IJwtTokenService tokenService,
        IUnitOfWork unitOfWork, RefreshTokenCommandHandler handler) CreateSut()
    {
        var users = Substitute.For<IUserRepository>();
        var refreshTokens = Substitute.For<IRefreshTokenRepository>();
        var tokenService = Substitute.For<IJwtTokenService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        tokenService.RefreshTokenLifetime.Returns(TimeSpan.FromDays(30));
        tokenService.GenerateAccessToken(Arg.Any<TokenClaims>()).Returns(new AccessToken("new-access-token", DateTimeOffset.UtcNow.AddMinutes(15)));
        tokenService.GenerateRefreshTokenValue().Returns("new-raw-refresh-token");
        tokenService.HashRefreshTokenValue(Arg.Any<string>()).Returns("hashed-value");
        var handler = new RefreshTokenCommandHandler(users, refreshTokens, tokenService, unitOfWork);
        return (users, refreshTokens, tokenService, unitOfWork, handler);
    }

    [Fact]
    public async Task Handle_WithActiveToken_RotatesAndReDerivesCurrentPermissions()
    {
        var (users, refreshTokens, _, unitOfWork, handler) = CreateSut();
        var companyId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = User.Register("owner@acme.com", "Owner", "hash");
        var existing = RefreshToken.Issue(user.Id, companyId, null, "hashed-value", DateTimeOffset.UtcNow.AddDays(1));
        refreshTokens.GetActiveByHashAsync("hashed-value", Arg.Any<CancellationToken>()).Returns(existing);
        users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        users.GetCompanyRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { (CompanyId: companyId, BranchId: (Guid?)null, RoleId: roleId) });
        users.GetRolePermissionCodesAsync(roleId, Arg.Any<CancellationToken>()).Returns(new[] { "core.company.read" });

        var result = await handler.Handle(new RefreshTokenCommand("old-raw-token"), CancellationToken.None);

        result.AccessToken.Should().Be("new-access-token");
        result.Permissions.Should().Contain("core.company.read");
        existing.IsActive.Should().BeFalse();
        await refreshTokens.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownOrExpiredToken_ThrowsValidationException()
    {
        var (_, refreshTokens, _, _, handler) = CreateSut();
        refreshTokens.GetActiveByHashAsync("hashed-value", Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        var act = () => handler.Handle(new RefreshTokenCommand("old-raw-token"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsDeactivated_ThrowsValidationException()
    {
        var (users, refreshTokens, _, _, handler) = CreateSut();
        var user = User.Register("owner@acme.com", "Owner", "hash");
        user.Deactivate();
        var existing = RefreshToken.Issue(user.Id, null, null, "hashed-value", DateTimeOffset.UtcNow.AddDays(1));
        refreshTokens.GetActiveByHashAsync("hashed-value", Arg.Any<CancellationToken>()).Returns(existing);
        users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var act = () => handler.Handle(new RefreshTokenCommand("old-raw-token"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
