using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Commands.Login;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Auth;

public class LoginCommandHandlerTests
{
    private static (IUserRepository users, IRefreshTokenRepository refreshTokens, IPasswordHasher hasher,
        IJwtTokenService tokenService, IUnitOfWork unitOfWork, LoginCommandHandler handler) CreateSut()
    {
        var users = Substitute.For<IUserRepository>();
        var refreshTokens = Substitute.For<IRefreshTokenRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var tokenService = Substitute.For<IJwtTokenService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        tokenService.RefreshTokenLifetime.Returns(TimeSpan.FromDays(30));
        tokenService.GenerateAccessToken(Arg.Any<TokenClaims>()).Returns(new AccessToken("access-token", DateTimeOffset.UtcNow.AddMinutes(15)));
        tokenService.GenerateRefreshTokenValue().Returns("raw-refresh-token");
        tokenService.HashRefreshTokenValue(Arg.Any<string>()).Returns("hashed-refresh-token");
        var handler = new LoginCommandHandler(users, refreshTokens, hasher, tokenService, unitOfWork);
        return (users, refreshTokens, hasher, tokenService, unitOfWork, handler);
    }

    [Fact]
    public async Task Handle_WithValidCredentialsAndSingleCompany_IssuesTokens()
    {
        var (users, refreshTokens, hasher, _, unitOfWork, handler) = CreateSut();
        var user = User.Register("owner@acme.com", "Owner", "hashed-password");
        var companyId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        users.GetByEmailAsync("owner@acme.com", Arg.Any<CancellationToken>()).Returns(user);
        hasher.Verify("correct-password", "hashed-password").Returns(true);
        users.GetCompanyRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { (CompanyId: companyId, BranchId: (Guid?)null, RoleId: roleId) });
        users.GetRolePermissionCodesAsync(roleId, Arg.Any<CancellationToken>()).Returns(new[] { "core.company.read" });

        var result = await handler.Handle(new LoginCommand("owner@acme.com", "correct-password", null), CancellationToken.None);

        result.CompanyId.Should().Be(companyId);
        result.Permissions.Should().Contain("core.company.read");
        result.AccessToken.Should().Be("access-token");
        await refreshTokens.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsValidationException()
    {
        var (users, _, hasher, _, _, handler) = CreateSut();
        var user = User.Register("owner@acme.com", "Owner", "hashed-password");
        users.GetByEmailAsync("owner@acme.com", Arg.Any<CancellationToken>()).Returns(user);
        hasher.Verify("wrong-password", "hashed-password").Returns(false);

        var act = () => handler.Handle(new LoginCommand("owner@acme.com", "wrong-password", null), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsValidationExceptionSameAsWrongPassword()
    {
        var (users, _, _, _, _, handler) = CreateSut();
        users.GetByEmailAsync("nobody@acme.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => handler.Handle(new LoginCommand("nobody@acme.com", "whatever", null), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenRequestedCompanyIsNotOneTheUserBelongsTo_ThrowsValidationException()
    {
        var (users, _, hasher, _, _, handler) = CreateSut();
        var user = User.Register("owner@acme.com", "Owner", "hashed-password");
        users.GetByEmailAsync("owner@acme.com", Arg.Any<CancellationToken>()).Returns(user);
        hasher.Verify("correct-password", "hashed-password").Returns(true);
        users.GetCompanyRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { (CompanyId: Guid.NewGuid(), BranchId: (Guid?)null, RoleId: Guid.NewGuid()) });

        var act = () => handler.Handle(new LoginCommand("owner@acme.com", "correct-password", Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithDeactivatedUser_ThrowsValidationException()
    {
        var (users, _, hasher, _, _, handler) = CreateSut();
        var user = User.Register("owner@acme.com", "Owner", "hashed-password");
        user.Deactivate();
        users.GetByEmailAsync("owner@acme.com", Arg.Any<CancellationToken>()).Returns(user);
        hasher.Verify("correct-password", "hashed-password").Returns(true);

        var act = () => handler.Handle(new LoginCommand("owner@acme.com", "correct-password", null), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
