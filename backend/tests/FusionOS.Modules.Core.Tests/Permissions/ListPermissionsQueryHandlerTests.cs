using FusionOS.Modules.Core.Application.Permissions.Queries.ListPermissions;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Permissions;

public class ListPermissionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheFullStaticPermissionCatalogSortedByModuleThenCode()
    {
        var handler = new ListPermissionsQueryHandler();

        var result = await handler.Handle(new ListPermissionsQuery(), CancellationToken.None);

        result.Should().NotBeEmpty();
        result.Should().Contain(p => p.Code == "core.company.read");
        result.Should().BeInAscendingOrder(p => p.Module).And.OnlyHaveUniqueItems(p => p.Code);
    }
}
