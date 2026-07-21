using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Users.Queries.ListCompanyUsers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Users;

public class ListCompanyUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsEveryUserLinkedToTheCompanyWithTheirCurrentRole()
    {
        var users = Substitute.For<IUserRepository>();
        var companyId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        users.ListCompanyUsersAsync(companyId, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new[] { (UserId: userId, Email: "owner@acme.com", FullName: "Owner", RoleId: roleId, RoleName: "Owner", IsActive: true) });
        var handler = new ListCompanyUsersQueryHandler(users);

        var result = await handler.Handle(new ListCompanyUsersQuery(companyId), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Email.Should().Be("owner@acme.com");
        result[0].RoleName.Should().Be("Owner");
        result[0].IsActive.Should().BeTrue();
    }
}
