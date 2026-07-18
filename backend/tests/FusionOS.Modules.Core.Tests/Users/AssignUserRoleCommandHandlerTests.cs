using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Users.Commands.AssignUserRole;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Users;

public class AssignUserRoleCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRoleExistsAndUserIsAlreadyAMember_AssignsRoleAndSaves()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = Role.CreateCompanyRole(companyId, "Warehouse Clerk");
        users.GetRoleByIdAsync(role.Id, companyId, Arg.Any<CancellationToken>()).Returns(role);
        users.GetCompanyRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new[] { (CompanyId: companyId, BranchId: (Guid?)null, RoleId: Guid.NewGuid()) });
        var handler = new AssignUserRoleCommandHandler(users, unitOfWork);

        await handler.Handle(new AssignUserRoleCommand(companyId, userId, role.Id), CancellationToken.None);

        await users.Received(1).AssignUserRoleAsync(userId, companyId, role.Id, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ThrowsKeyNotFoundException()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        users.GetRoleByIdAsync(roleId, companyId, Arg.Any<CancellationToken>()).Returns((Role?)null);
        var handler = new AssignUserRoleCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new AssignUserRoleCommand(companyId, Guid.NewGuid(), roleId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAMemberOfTheCompany_ThrowsKeyNotFoundException()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = Role.CreateCompanyRole(companyId, "Warehouse Clerk");
        users.GetRoleByIdAsync(role.Id, companyId, Arg.Any<CancellationToken>()).Returns(role);
        users.GetCompanyRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new[] { (CompanyId: Guid.NewGuid(), BranchId: (Guid?)null, RoleId: Guid.NewGuid()) });
        var handler = new AssignUserRoleCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new AssignUserRoleCommand(companyId, userId, role.Id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await users.DidNotReceive().AssignUserRoleAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
