using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Attributes.Queries.GetAttributeDefinitionById;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class GetAttributeDefinitionByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenDefinitionExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var definition = AttributeDefinition.Create(companyId, "Color");
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.GetByIdAsync(companyId, definition.Id, Arg.Any<CancellationToken>()).Returns(definition);
        var handler = new GetAttributeDefinitionByIdQueryHandler(repository);

        var result = await handler.Handle(new GetAttributeDefinitionByIdQuery(companyId, definition.Id), CancellationToken.None);

        result.Name.Should().Be("Color");
    }

    [Fact]
    public async Task Handle_WhenDefinitionDoesNotExist_Throws()
    {
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AttributeDefinition?)null);
        var handler = new GetAttributeDefinitionByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetAttributeDefinitionByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
