using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class DeactivateAttributeDefinitionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDefinitionExists_Deactivates()
    {
        var companyId = Guid.NewGuid();
        var definition = AttributeDefinition.Create(companyId, "Color");
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.GetByIdAsync(companyId, definition.Id, Arg.Any<CancellationToken>()).Returns(definition);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateAttributeDefinitionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateAttributeDefinitionCommand(companyId, definition.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDefinitionDoesNotExist_Throws()
    {
        var repository = Substitute.For<IAttributeDefinitionRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AttributeDefinition?)null);
        var handler = new DeactivateAttributeDefinitionCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateAttributeDefinitionCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
