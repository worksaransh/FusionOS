using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProduct;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Products;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Products;

public class DeactivateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductBelongsToCompany_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "SKU-1", "Widget", "PCS");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateProductCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateProductCommand(companyId, product.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductBelongsToDifferentCompany_ThrowsValidationException()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Widget", "PCS");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateProductCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateProductCommand(Guid.NewGuid(), product.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateProductCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateProductCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
