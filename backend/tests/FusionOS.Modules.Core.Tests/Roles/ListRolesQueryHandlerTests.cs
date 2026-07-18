using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Roles.Queries.ListRoles;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Roles;

public class ListRolesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllRolesForTheCompany()
    {
        var users = Substitute.For<IUserRepository>();
        var companyId = Guid.NewGuid();
        var roles = new[]
        {
            Role.CreateCompanyRole(companyId, "Owner"),
            Role.CreateCompanyRole(companyId, "Warehouse Clerk"),
        };
        users.ListRolesByCompanyAsync(companyId, Arg.Any<CancellationToken>()).Returns(roles);
        var handler = new ListRolesQueryHandler(users);

        var result = await handler.Handle(new ListRolesQuery(companyId), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().Contain(new[] { "Owner", "Warehouse Clerk" });
    }
}
