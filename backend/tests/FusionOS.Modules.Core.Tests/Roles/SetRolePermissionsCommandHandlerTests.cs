using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Roles.Commands.SetRolePermissions;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Roles;

public class SetRolePermissionsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRoleExists_ReplacesPermissionsAndReturnsUpdatedSet()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var role = Role.CreateCompanyRole(companyId, "Warehouse Clerk");
        var codes = new[] { "warehouse.warehouse.read", "warehouse.zone.read" };
        users.GetRoleByIdAsync(role.Id, companyId, Arg.Any<CancellationToken>()).Returns(role);
        users.GetRolePermissionCodesAsync(role.Id, Arg.Any<CancellationToken>()).Returns(codes);
        var handler = new SetRolePermissionsCommandHandler(users, unitOfWork);

        var result = await handler.Handle(new SetRolePermissionsCommand(companyId, role.Id, codes), CancellationToken.None);

        result.Should().BeEquivalentTo(codes);
        await users.Received(1).SetRolePermissionsAsync(role.Id, codes, Arg.Any<CancellationToken>());
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
        var handler = new SetRolePermissionsCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new SetRolePermissionsCommand(companyId, roleId, new[] { "core.company.read" }), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAPermissionCodeIsNotInTheCatalog_ThrowsValidationException()
    {
        // PermissionCatalog is meant to be the single source of truth for every
        // permission code IRequirePermission can ever check - an unknown string
        // assigned to a role would silently be a no-op grant forever.
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        var role = Role.CreateCompanyRole(companyId, "Warehouse Clerk");
        users.GetRoleByIdAsync(role.Id, companyId, Arg.Any<CancellationToken>()).Returns(role);
        var handler = new SetRolePermissionsCommandHandler(users, unitOfWork);
        var command = new SetRolePermissionsCommand(companyId, role.Id, new[] { "warehouse.warehouse.read", "not.a.real.code" });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await users.DidNotReceive().SetRolePermissionsAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
    }
}
