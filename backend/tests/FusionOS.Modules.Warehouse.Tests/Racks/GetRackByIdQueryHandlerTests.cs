using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using FusionOS.Modules.Warehouse.Application.Racks.Queries.GetRackById;
using FusionOS.Modules.Warehouse.Domain.Racks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Racks;

public class GetRackByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenRackBelongsToCompany_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var rack = Rack.Create(companyId, Guid.NewGuid(), "Aisle 3", "R-01");
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(rack.Id, Arg.Any<CancellationToken>()).Returns(rack);
        var handler = new GetRackByIdQueryHandler(repository);

        var result = await handler.Handle(new GetRackByIdQuery(companyId, rack.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("R-01");
    }

    [Fact]
    public async Task Handle_WhenRackBelongsToDifferentCompany_ReturnsNull()
    {
        var rack = Rack.Create(Guid.NewGuid(), Guid.NewGuid(), "Aisle 3", "R-01");
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(rack.Id, Arg.Any<CancellationToken>()).Returns(rack);
        var handler = new GetRackByIdQueryHandler(repository);

        var result = await handler.Handle(new GetRackByIdQuery(Guid.NewGuid(), rack.Id), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRackDoesNotExist_ReturnsNull()
    {
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Rack?)null);
        var handler = new GetRackByIdQueryHandler(repository);

        var result = await handler.Handle(new GetRackByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
