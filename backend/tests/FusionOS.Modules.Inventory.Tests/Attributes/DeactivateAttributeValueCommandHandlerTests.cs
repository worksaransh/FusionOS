using FluentAssertions;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeValue;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class DeactivateAttributeValueCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenValueExists_Deactivates()
    {
        var companyId = Guid.NewGuid();
        var value = AttributeValue.Create(companyId, Guid.NewGuid(), "Red");
        var repository = Substitute.For<IAttributeValueRepository>();
        repository.GetByIdAsync(companyId, value.Id, Arg.Any<CancellationToken>()).Returns(value);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateAttributeValueCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateAttributeValueCommand(companyId, value.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValueDoesNotExist_Throws()
    {
        var repository = Substitute.For<IAttributeValueRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AttributeValue?)null);
        var handler = new DeactivateAttributeValueCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateAttributeValueCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
