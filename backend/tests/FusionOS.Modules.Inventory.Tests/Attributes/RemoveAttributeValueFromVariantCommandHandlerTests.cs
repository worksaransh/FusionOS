using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.RemoveAttributeValueFromVariant;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class RemoveAttributeValueFromVariantCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAssignmentExists_RemovesIt()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var assignment = ProductVariantAttributeValue.Create(companyId, productId, variantId, Guid.NewGuid(), Guid.NewGuid());
        var repository = Substitute.For<IProductVariantAttributeValueRepository>();
        repository.GetByIdAsync(companyId, assignment.Id, Arg.Any<CancellationToken>()).Returns(assignment);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RemoveAttributeValueFromVariantCommandHandler(repository, unitOfWork);

        await handler.Handle(new RemoveAttributeValueFromVariantCommand(companyId, productId, variantId, assignment.Id), CancellationToken.None);

        repository.Received(1).Remove(assignment);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAssignmentDoesNotExist_Throws()
    {
        var repository = Substitute.For<IProductVariantAttributeValueRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProductVariantAttributeValue?)null);
        var handler = new RemoveAttributeValueFromVariantCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new RemoveAttributeValueFromVariantCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenAssignmentBelongsToDifferentVariant_Throws()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var assignment = ProductVariantAttributeValue.Create(companyId, productId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var repository = Substitute.For<IProductVariantAttributeValueRepository>();
        repository.GetByIdAsync(companyId, assignment.Id, Arg.Any<CancellationToken>()).Returns(assignment);
        var handler = new RemoveAttributeValueFromVariantCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new RemoveAttributeValueFromVariantCommand(companyId, productId, Guid.NewGuid(), assignment.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
