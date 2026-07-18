using FusionOS.Modules.Core.Application.Auth.Commands.Logout;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Auth;

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTokenIsActive_RevokesItAndSaves()
    {
        var refreshTokens = Substitute.For<IRefreshTokenRepository>();
        var tokenService = Substitute.For<IJwtTokenService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        tokenService.HashRefreshTokenValue("raw-token").Returns("hashed-token");
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), null, "hashed-token", DateTimeOffset.UtcNow.AddDays(1));
        refreshTokens.GetActiveByHashAsync("hashed-token", Arg.Any<CancellationToken>()).Returns(token);
        var handler = new LogoutCommandHandler(refreshTokens, tokenService, unitOfWork);

        await handler.Handle(new LogoutCommand("raw-token"), CancellationToken.None);

        token.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenIsUnknown_IsANoOpAndDoesNotSave()
    {
        var refreshTokens = Substitute.For<IRefreshTokenRepository>();
        var tokenService = Substitute.For<IJwtTokenService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        tokenService.HashRefreshTokenValue("raw-token").Returns("hashed-token");
        refreshTokens.GetActiveByHashAsync("hashed-token", Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);
        var handler = new LogoutCommandHandler(refreshTokens, tokenService, unitOfWork);

        await handler.Handle(new LogoutCommand("raw-token"), CancellationToken.None);

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
