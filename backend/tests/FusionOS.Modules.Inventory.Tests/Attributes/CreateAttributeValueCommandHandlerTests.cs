using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeValue;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class CreateAttributeValueCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDefinitionExistsAndValueIsNew_PersistsAttributeValue()
    {
        var companyId = Guid.NewGuid();
        var definition = AttributeDefinition.Create(companyId, "Color");
        var definitionRepository = Substitute.For<IAttributeDefinitionRepository>();
        definitionRepository.GetByIdAsync(companyId, definition.Id, Arg.Any<CancellationToken>()).Returns(definition);
        var valueRepository = Substitute.For<IAttributeValueRepository>();
        valueRepository.ValueExistsAsync(definition.Id, "Red", Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAttributeValueCommandHandler(valueRepository, definitionRepository, unitOfWork);

        var result = await handler.Handle(new CreateAttributeValueCommand(companyId, definition.Id, "Red"), CancellationToken.None);

        result.Value.Should().Be("Red");
        result.AttributeDefinitionId.Should().Be(definition.Id);
        await valueRepository.Received(1).AddAsync(Arg.Any<AttributeValue>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDefinitionDoesNotExist_Throws()
    {
        var definitionRepository = Substitute.For<IAttributeDefinitionRepository>();
        definitionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AttributeDefinition?)null);
        var handler = new CreateAttributeValueCommandHandler(Substitute.For<IAttributeValueRepository>(), definitionRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CreateAttributeValueCommand(Guid.NewGuid(), Guid.NewGuid(), "Red"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenValueAlreadyExistsForDefinition_Throws()
    {
        var companyId = Guid.NewGuid();
        var definition = AttributeDefinition.Create(companyId, "Color");
        var definitionRepository = Substitute.For<IAttributeDefinitionRepository>();
        definitionRepository.GetByIdAsync(companyId, definition.Id, Arg.Any<CancellationToken>()).Returns(definition);
        var valueRepository = Substitute.For<IAttributeValueRepository>();
        valueRepository.ValueExistsAsync(definition.Id, "Red", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateAttributeValueCommandHandler(valueRepository, definitionRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CreateAttributeValueCommand(companyId, definition.Id, "Red"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
