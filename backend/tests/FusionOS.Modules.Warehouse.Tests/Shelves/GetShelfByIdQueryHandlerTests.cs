using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using FusionOS.Modules.Warehouse.Application.Shelves.Queries.GetShelfById;
using FusionOS.Modules.Warehouse.Domain.Shelves;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Shelves;

public class GetShelfByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenShelfBelongsToCompany_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var shelf = Shelf.Create(companyId, Guid.NewGuid(), "Top Shelf", "S-01");
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(shelf.Id, Arg.Any<CancellationToken>()).Returns(shelf);
        var handler = new GetShelfByIdQueryHandler(repository);

        var result = await handler.Handle(new GetShelfByIdQuery(companyId, shelf.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("S-01");
    }

    [Fact]
    public async Task Handle_WhenShelfBelongsToDifferentCompany_ReturnsNull()
    {
        var shelf = Shelf.Create(Guid.NewGuid(), Guid.NewGuid(), "Top Shelf", "S-01");
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(shelf.Id, Arg.Any<CancellationToken>()).Returns(shelf);
        var handler = new GetShelfByIdQueryHandler(repository);

        var result = await handler.Handle(new GetShelfByIdQuery(Guid.NewGuid(), shelf.Id), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenShelfDoesNotExist_ReturnsNull()
    {
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Shelf?)null);
        var handler = new GetShelfByIdQueryHandler(repository);

        var result = await handler.Handle(new GetShelfByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
