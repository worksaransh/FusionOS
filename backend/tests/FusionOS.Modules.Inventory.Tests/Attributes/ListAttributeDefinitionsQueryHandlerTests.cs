using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Attributes.Queries.ListAttributeDefinitions;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class ListAttributeDefinitionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedDefinitionsForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var definitions = new[] { AttributeDefinition.Create(companyId, "Color") };
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(definitions);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListAttributeDefinitionsQueryHandler(repository);

        var result = await handler.Handle(new ListAttributeDefinitionsQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(d => d.Name == "Color");
    }
}
