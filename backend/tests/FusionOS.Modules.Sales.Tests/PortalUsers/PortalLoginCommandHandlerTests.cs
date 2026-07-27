using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.PortalUsers.Commands.PortalLogin;
using FusionOS.Modules.Sales.Application.PortalUsers.Contracts;
using FusionOS.Modules.Sales.Domain.PortalUsers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.PortalUsers;

public class PortalLoginCommandHandlerTests
{
    private static CustomerPortalUser ActivePortalUser(Guid companyId, Guid customerId, string email, string passwordHash) =>
        CustomerPortalUser.Create(companyId, customerId, email, passwordHash);

    [Fact]
    public async Task Handle_WithCorrectPassword_IssuesTokenForMatchingCustomer()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var portalUser = ActivePortalUser(companyId, customerId, "contact@example.com", "stored-hash");

        var repository = Substitute.For<ICustomerPortalUserRepository>();
        repository.GetByEmailAsync("contact@example.com", Arg.Any<CancellationToken>())
            .Returns(new[] { portalUser });

        var hasher = Substitute.For<IPortalPasswordHasher>();
        hasher.Verify("correct-password", "stored-hash").Returns(true);

        var tokenService = Substitute.For<IPortalTokenService>();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        tokenService.GenerateAccessToken(Arg.Is<PortalTokenClaims>(c => c.PortalUserId == portalUser.Id && c.CustomerId == customerId))
            .Returns(new PortalAccessToken("a.jwt.token", expiresAt));

        var handler = new PortalLoginCommandHandler(repository, hasher, tokenService);

        var result = await handler.Handle(new PortalLoginCommand("contact@example.com", "correct-password"), CancellationToken.None);

        result.CustomerId.Should().Be(customerId);
        result.PortalUserId.Should().Be(portalUser.Id);
        result.AccessToken.Should().Be("a.jwt.token");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsValidationException()
    {
        var portalUser = ActivePortalUser(Guid.NewGuid(), Guid.NewGuid(), "contact@example.com", "stored-hash");

        var repository = Substitute.For<ICustomerPortalUserRepository>();
        repository.GetByEmailAsync("contact@example.com", Arg.Any<CancellationToken>())
            .Returns(new[] { portalUser });

        var hasher = Substitute.For<IPortalPasswordHasher>();
        hasher.Verify("wrong-password", "stored-hash").Returns(false);

        var tokenService = Substitute.For<IPortalTokenService>();
        var handler = new PortalLoginCommandHandler(repository, hasher, tokenService);

        var act = () => handler.Handle(new PortalLoginCommand("contact@example.com", "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        tokenService.DidNotReceive().GenerateAccessToken(Arg.Any<PortalTokenClaims>());
    }

    [Fact]
    public async Task Handle_WithInactiveAccount_ThrowsValidationException_EvenWithCorrectPassword()
    {
        var portalUser = ActivePortalUser(Guid.NewGuid(), Guid.NewGuid(), "contact@example.com", "stored-hash");
        portalUser.Deactivate();

        var repository = Substitute.For<ICustomerPortalUserRepository>();
        repository.GetByEmailAsync("contact@example.com", Arg.Any<CancellationToken>())
            .Returns(new[] { portalUser });

        var hasher = Substitute.For<IPortalPasswordHasher>();
        hasher.Verify("correct-password", "stored-hash").Returns(true);

        var tokenService = Substitute.For<IPortalTokenService>();
        var handler = new PortalLoginCommandHandler(repository, hasher, tokenService);

        var act = () => handler.Handle(new PortalLoginCommand("contact@example.com", "correct-password"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        tokenService.DidNotReceive().GenerateAccessToken(Arg.Any<PortalTokenClaims>());
    }

    [Fact]
    public async Task Handle_WithNoMatchingEmail_ThrowsValidationException()
    {
        var repository = Substitute.For<ICustomerPortalUserRepository>();
        repository.GetByEmailAsync("unknown@example.com", Arg.Any<CancellationToken>())
            .Returns(Array.Empty<CustomerPortalUser>());

        var hasher = Substitute.For<IPortalPasswordHasher>();
        var tokenService = Substitute.For<IPortalTokenService>();
        var handler = new PortalLoginCommandHandler(repository, hasher, tokenService);

        var act = () => handler.Handle(new PortalLoginCommand("unknown@example.com", "whatever"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    /// <summary>
    /// The same email can legitimately belong to two different Customers
    /// (CustomerPortalUser's own doc comment) - this confirms the handler
    /// picks whichever candidate's password actually matches, not just the
    /// first row returned, and that a correct password for customer A never
    /// authenticates as customer B.
    /// </summary>
    [Fact]
    public async Task Handle_WithSameEmailAcrossTwoCustomers_MatchesOnlyTheCandidateWithTheCorrectPassword()
    {
        var customerA = ActivePortalUser(Guid.NewGuid(), Guid.NewGuid(), "shared@example.com", "hash-a");
        var customerB = ActivePortalUser(Guid.NewGuid(), Guid.NewGuid(), "shared@example.com", "hash-b");

        var repository = Substitute.For<ICustomerPortalUserRepository>();
        repository.GetByEmailAsync("shared@example.com", Arg.Any<CancellationToken>())
            .Returns(new[] { customerA, customerB });

        var hasher = Substitute.For<IPortalPasswordHasher>();
        hasher.Verify("password-for-b", "hash-a").Returns(false);
        hasher.Verify("password-for-b", "hash-b").Returns(true);

        var tokenService = Substitute.For<IPortalTokenService>();
        tokenService.GenerateAccessToken(Arg.Any<PortalTokenClaims>())
            .Returns(new PortalAccessToken("token", DateTimeOffset.UtcNow.AddMinutes(15)));

        var handler = new PortalLoginCommandHandler(repository, hasher, tokenService);

        var result = await handler.Handle(new PortalLoginCommand("shared@example.com", "password-for-b"), CancellationToken.None);

        result.CustomerId.Should().Be(customerB.CustomerId);
    }
}
