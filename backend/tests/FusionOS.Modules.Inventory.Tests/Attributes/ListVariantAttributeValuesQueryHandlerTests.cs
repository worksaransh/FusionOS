using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Attributes.Queries.ListVariantAttributeValues;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class ListVariantAttributeValuesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAssignmentsWithResolvedNames()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var definition = AttributeDefinition.Create(companyId, "Color");
        var value = AttributeValue.Create(companyId, definition.Id, "Red");
        var assignment = ProductVariantAttributeValue.Create(companyId, productId, variantId, definition.Id, value.Id);

        var assignmentRepository = Substitute.For<IProductVariantAttributeValueRepository>();
        assignmentRepository.ListForVariantAsync(variantId, Arg.Any<CancellationToken>()).Returns(new[] { assignment });
        var definitionRepository = Substitute.For<IAttributeDefinitionRepository>();
        definitionRepository.GetByIdAsync(companyId, definition.Id, Arg.Any<CancellationToken>()).Returns(definition);
        var valueRepository = Substitute.For<IAttributeValueRepository>();
        valueRepository.GetByIdAsync(companyId, value.Id, Arg.Any<CancellationToken>()).Returns(value);

        var handler = new ListVariantAttributeValuesQueryHandler(assignmentRepository, definitionRepository, valueRepository);

        var result = await handler.Handle(new ListVariantAttributeValuesQuery(companyId, productId, variantId), CancellationToken.None);

        result.Should().ContainSingle(a => a.AttributeDefinitionName == "Color" && a.AttributeValue == "Red");
    }
}
