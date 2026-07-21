using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using FusionOS.Modules.Warehouse.Application.Shelves.Queries.ListShelves;
using FusionOS.Modules.Warehouse.Domain.Shelves;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Shelves;

public class ListShelvesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedShelvesForTheRack()
    {
        var companyId = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var shelves = new[] { Shelf.Create(companyId, rackId, "Top Shelf", "S-01") };
        var repository = Substitute.For<IShelfRepository>();
        repository.ListAsync(companyId, rackId, 1, 25, Arg.Any<CancellationToken>()).Returns(shelves);
        repository.CountAsync(companyId, rackId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListShelvesQueryHandler(repository);

        var result = await handler.Handle(new ListShelvesQuery(companyId, rackId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.Code == "S-01");
    }

    [Fact]
    public async Task Handle_ScopesToTheGivenRackId()
    {
        var companyId = Guid.NewGuid();
        var rackId = Guid.NewGuid();
        var repository = Substitute.For<IShelfRepository>();
        repository.ListAsync(companyId, rackId, 1, 25, Arg.Any<CancellationToken>()).Returns(Array.Empty<Shelf>());
        repository.CountAsync(companyId, rackId, Arg.Any<CancellationToken>()).Returns(0);
        var handler = new ListShelvesQueryHandler(repository);

        await handler.Handle(new ListShelvesQuery(companyId, rackId), CancellationToken.None);

        await repository.Received(1).ListAsync(companyId, rackId, 1, 25, Arg.Any<CancellationToken>());
    }
}
