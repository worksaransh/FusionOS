using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Roles.Queries.GetRolePermissions;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Roles;

public class GetRolePermissionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenRoleExists_ReturnsItsPermissionCodes()
    {
        var users = Substitute.For<IUserRepository>();
        var companyId = Guid.NewGuid();
        var role = Role.CreateCompanyRole(companyId, "Warehouse Clerk");
        var codes = new[] { "warehouse.warehouse.read" };
        users.GetRoleByIdAsync(role.Id, companyId, Arg.Any<CancellationToken>()).Returns(role);
        users.GetRolePermissionCodesAsync(role.Id, Arg.Any<CancellationToken>()).Returns(codes);
        var handler = new GetRolePermissionsQueryHandler(users);

        var result = await handler.Handle(new GetRolePermissionsQuery(companyId, role.Id), CancellationToken.None);

        result.Should().BeEquivalentTo(codes);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ThrowsKeyNotFoundException()
    {
        var users = Substitute.For<IUserRepository>();
        var companyId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        users.GetRoleByIdAsync(roleId, companyId, Arg.Any<CancellationToken>()).Returns((Role?)null);
        var handler = new GetRolePermissionsQueryHandler(users);

        var act = () => handler.Handle(new GetRolePermissionsQuery(companyId, roleId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
