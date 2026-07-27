using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.UpdateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class UpdateAttributeDefinitionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDefinitionExists_UpdatesName()
    {
        var companyId = Guid.NewGuid();
        var definition = AttributeDefinition.Create(companyId, "Color");
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.GetByIdAsync(companyId, definition.Id, Arg.Any<CancellationToken>()).Returns(definition);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateAttributeDefinitionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new UpdateAttributeDefinitionCommand(companyId, definition.Id, "Colour"), CancellationToken.None);

        result.Name.Should().Be("Colour");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDefinitionDoesNotExist_Throws()
    {
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AttributeDefinition?)null);
        var handler = new UpdateAttributeDefinitionCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new UpdateAttributeDefinitionCommand(Guid.NewGuid(), Guid.NewGuid(), "Colour"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
