using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Commands.Register;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Identity;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Auth;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ForBrandNewCompanyWithNoUsers_CreatesBootstrapOwner()
    {
        var users = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        var companyId = Guid.NewGuid();
        var ownerRoleId = Guid.NewGuid();
        users.CompanyHasAnyUsersAsync(companyId, Arg.Any<CancellationToken>()).Returns(false);
        users.GetByEmailAsync("first@acme.com", Arg.Any<CancellationToken>()).Returns((User?)null);
        hasher.Hash("password123").Returns("hashed-password");
        users.GetOrCreateCompanyOwnerRoleAsync(companyId, Arg.Any<CancellationToken>()).Returns(ownerRoleId);
        var handler = new RegisterUserCommandHandler(users, hasher, unitOfWork, currentUser);

        var result = await handler.Handle(new RegisterUserCommand("first@acme.com", "First Owner", "password123", companyId), CancellationToken.None);

        result.Email.Should().Be("first@acme.com");
        await users.Received(1).LinkUserToCompanyAsync(result.Id, companyId, ownerRoleId, null, Arg.Any<CancellationToken>());
        await users.DidNotReceive().GetOrCreateDefaultMemberRoleAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ForExistingCompanyWithPermission_InvitesAsMember()
    {
        var users = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        var companyId = Guid.NewGuid();
        var memberRoleId = Guid.NewGuid();
        currentUser.HasPermission("core.user.register").Returns(true);
        users.CompanyHasAnyUsersAsync(companyId, Arg.Any<CancellationToken>()).Returns(true);
        users.GetByEmailAsync("teammate@acme.com", Arg.Any<CancellationToken>()).Returns((User?)null);
        hasher.Hash("password123").Returns("hashed-password");
        users.GetOrCreateDefaultMemberRoleAsync(companyId, Arg.Any<CancellationToken>()).Returns(memberRoleId);
        var handler = new RegisterUserCommandHandler(users, hasher, unitOfWork, currentUser);

        var result = await handler.Handle(new RegisterUserCommand("teammate@acme.com", "Teammate", "password123", companyId), CancellationToken.None);

        result.Email.Should().Be("teammate@acme.com");
        await users.Received(1).LinkUserToCompanyAsync(result.Id, companyId, memberRoleId, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ForExistingCompanyWithoutPermission_ThrowsForbiddenException()
    {
        var users = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        var companyId = Guid.NewGuid();
        currentUser.HasPermission("core.user.register").Returns(false);
        users.CompanyHasAnyUsersAsync(companyId, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new RegisterUserCommandHandler(users, hasher, unitOfWork, currentUser);

        var act = () => handler.Handle(new RegisterUserCommand("teammate@acme.com", "Teammate", "password123", companyId), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyRegisteredEmail_ThrowsValidationException()
    {
        var users = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        var companyId = Guid.NewGuid();
        var existingUser = User.Register("first@acme.com", "First Owner", "hashed-password");
        currentUser.HasPermission("core.user.register").Returns(true);
        users.CompanyHasAnyUsersAsync(companyId, Arg.Any<CancellationToken>()).Returns(true);
        users.GetByEmailAsync("first@acme.com", Arg.Any<CancellationToken>()).Returns(existingUser);
        var handler = new RegisterUserCommandHandler(users, hasher, unitOfWork, currentUser);

        var act = () => handler.Handle(new RegisterUserCommand("first@acme.com", "Duplicate", "password123", companyId), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
