using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Users.Commands.DeactivateUser;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Users;

public class DeactivateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAMemberOfTheCompany_DeactivatesAndSaves()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var user = User.Register("clerk@acme.com", "Clerk", "hash");
        users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        users.GetCompanyRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { (CompanyId: companyId, BranchId: (Guid?)null, RoleId: Guid.NewGuid()) });
        var handler = new DeactivateUserCommandHandler(users, unitOfWork);

        await handler.Handle(new DeactivateUserCommand(companyId, user.Id), CancellationToken.None);

        user.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsKeyNotFoundException()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);
        var handler = new DeactivateUserCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new DeactivateUserCommand(companyId, userId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAMemberOfTheCompany_ThrowsKeyNotFoundException()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var user = User.Register("clerk@acme.com", "Clerk", "hash");
        users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        users.GetCompanyRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { (CompanyId: Guid.NewGuid(), BranchId: (Guid?)null, RoleId: Guid.NewGuid()) });
        var handler = new DeactivateUserCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new DeactivateUserCommand(companyId, user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        user.IsActive.Should().BeTrue();
    }
}
