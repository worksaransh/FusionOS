using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class CreateAttributeDefinitionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNameIsNew_PersistsAttributeDefinition()
    {
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.NameExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAttributeDefinitionCommandHandler(repository, unitOfWork);
        var command = new CreateAttributeDefinitionCommand(Guid.NewGuid(), "Color");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Color");
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Inventory.Domain.Attributes.AttributeDefinition>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_Throws()
    {
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.NameExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAttributeDefinitionCommandHandler(repository, unitOfWork);
        var command = new CreateAttributeDefinitionCommand(Guid.NewGuid(), "Color");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
