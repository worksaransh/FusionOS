using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Attributes.Queries.ListAttributeValuesByDefinition;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class ListAttributeValuesByDefinitionQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsValuesForTheDefinition()
    {
        var companyId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        var values = new[] { AttributeValue.Create(companyId, definitionId, "Red") };
        var repository = Substitute.For<IAttributeValueRepository>();
        repository.ListByDefinitionAsync(companyId, definitionId, Arg.Any<CancellationToken>()).Returns(values);
        var handler = new ListAttributeValuesByDefinitionQueryHandler(repository);

        var result = await handler.Handle(new ListAttributeValuesByDefinitionQuery(companyId, definitionId), CancellationToken.None);

        result.Should().ContainSingle(v => v.Value == "Red");
    }
}
