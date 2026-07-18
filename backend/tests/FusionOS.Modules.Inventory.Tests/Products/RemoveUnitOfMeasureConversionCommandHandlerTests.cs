using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Products.Commands.RemoveUnitOfMeasureConversion;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Products;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Products;

public class RemoveUnitOfMeasureConversionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenConversionExists_RemovesItAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "SKU-1", "Widget", "PCS");
        product.AddUnitOfMeasureConversion("BOX", 12);
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RemoveUnitOfMeasureConversionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new RemoveUnitOfMeasureConversionCommand(companyId, product.Id, "BOX"), CancellationToken.None);

        result.UnitOfMeasureConversions.Should().BeEmpty();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductBelongsToDifferentCompany_ThrowsValidationException()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Widget", "PCS");
        product.AddUnitOfMeasureConversion("BOX", 12);
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RemoveUnitOfMeasureConversionCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new RemoveUnitOfMeasureConversionCommand(Guid.NewGuid(), product.Id, "BOX"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RemoveUnitOfMeasureConversionCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new RemoveUnitOfMeasureConversionCommand(Guid.NewGuid(), Guid.NewGuid(), "BOX"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
