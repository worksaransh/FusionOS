using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Roles.Commands.UpdateRole;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Roles;

public class UpdateRoleCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithUniqueName_RenamesTheRole()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var role = Role.CreateCompanyRole(companyId, "Warehouse Clerk");
        users.GetRoleByIdAsync(role.Id, companyId, Arg.Any<CancellationToken>()).Returns(role);
        users.RoleNameExistsAsync(companyId, "Senior Warehouse Clerk", role.Id, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new UpdateRoleCommandHandler(users, unitOfWork);

        var result = await handler.Handle(new UpdateRoleCommand(companyId, role.Id, "Senior Warehouse Clerk"), CancellationToken.None);

        result.Name.Should().Be("Senior Warehouse Clerk");
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
        var handler = new UpdateRoleCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new UpdateRoleCommand(companyId, roleId, "New Name"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyUsedByAnotherRole_ThrowsValidationException()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var role = Role.CreateCompanyRole(companyId, "Warehouse Clerk");
        users.GetRoleByIdAsync(role.Id, companyId, Arg.Any<CancellationToken>()).Returns(role);
        users.RoleNameExistsAsync(companyId, "Owner", role.Id, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new UpdateRoleCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new UpdateRoleCommand(companyId, role.Id, "Owner"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRoleIsASystemRole_ThrowsValidationException()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var role = Role.CreateSystemRole("Owner");
        users.GetRoleByIdAsync(role.Id, companyId, Arg.Any<CancellationToken>()).Returns(role);
        users.RoleNameExistsAsync(companyId, "Not Owner", role.Id, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new UpdateRoleCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new UpdateRoleCommand(companyId, role.Id, "Not Owner"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
