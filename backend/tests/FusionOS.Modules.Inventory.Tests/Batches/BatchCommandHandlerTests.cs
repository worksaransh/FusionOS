using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Batches.Commands.ConsumeBatch;
using FusionOS.Modules.Inventory.Application.Batches.Commands.CreateBatch;
using FusionOS.Modules.Inventory.Application.Batches.Commands.UpdateBatchExpiry;
using FusionOS.Modules.Inventory.Application.Batches.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Batches;
using FusionOS.Modules.Inventory.Domain.Products;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Batches;

public class BatchCommandHandlerTests
{
    [Fact]
    public async Task CreateBatch_WhenProductExists_PersistsBatch()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "WIDGET", "Widget", "PCS");
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var repository = Substitute.For<IBatchRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBatchCommandHandler(repository, productRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateBatchCommand(companyId, product.Id, "B-001", 50m, null),
            CancellationToken.None);

        result.QuantityReceived.Should().Be(50m);
        result.QuantityRemaining.Should().Be(50m);
        await repository.Received(1).AddAsync(Arg.Any<Batch>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBatch_WhenProductDoesNotExist_ThrowsValidationException()
    {
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var handler = new CreateBatchCommandHandler(Substitute.For<IBatchRepository>(), productRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CreateBatchCommand(Guid.NewGuid(), Guid.NewGuid(), "B-001", 50m, null), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateBatch_WhenBatchNumberAlreadyExists_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "WIDGET", "Widget", "PCS");
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var repository = Substitute.For<IBatchRepository>();
        repository.BatchNumberExistsAsync(companyId, product.Id, "B-001", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateBatchCommandHandler(repository, productRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CreateBatchCommand(companyId, product.Id, "B-001", 50m, null), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ConsumeBatch_ReducesQuantityRemaining()
    {
        var companyId = Guid.NewGuid();
        var batch = Batch.Create(companyId, Guid.NewGuid(), "B-001", 100m);
        var repository = Substitute.For<IBatchRepository>();
        repository.GetByIdAsync(companyId, batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        var handler = new ConsumeBatchCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new ConsumeBatchCommand(companyId, batch.Id, 30m), CancellationToken.None);

        result.QuantityRemaining.Should().Be(70m);
    }

    [Fact]
    public async Task ConsumeBatch_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IBatchRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Batch?)null);
        var handler = new ConsumeBatchCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new ConsumeBatchCommand(companyId, Guid.NewGuid(), 10m), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ConsumeBatch_MoreThanRemaining_Throws()
    {
        var companyId = Guid.NewGuid();
        var batch = Batch.Create(companyId, Guid.NewGuid(), "B-001", 10m);
        var repository = Substitute.For<IBatchRepository>();
        repository.GetByIdAsync(companyId, batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        var handler = new ConsumeBatchCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new ConsumeBatchCommand(companyId, batch.Id, 20m), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateBatchExpiry_UpdatesExpiryDate()
    {
        var companyId = Guid.NewGuid();
        var batch = Batch.Create(companyId, Guid.NewGuid(), "B-001", 10m);
        var repository = Substitute.For<IBatchRepository>();
        repository.GetByIdAsync(companyId, batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        var handler = new UpdateBatchExpiryCommandHandler(repository, Substitute.For<IUnitOfWork>());
        var newExpiry = DateTimeOffset.UtcNow.AddMonths(3);

        var result = await handler.Handle(new UpdateBatchExpiryCommand(companyId, batch.Id, newExpiry), CancellationToken.None);

        result.ExpiryDate.Should().Be(newExpiry);
    }

    [Fact]
    public async Task UpdateBatchExpiry_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IBatchRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Batch?)null);
        var handler = new UpdateBatchExpiryCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new UpdateBatchExpiryCommand(companyId, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
