using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Transfers.Commands.CancelTransfer;
using FusionOS.Modules.Inventory.Application.Transfers.Commands.CompleteTransfer;
using FusionOS.Modules.Inventory.Application.Transfers.Commands.CreateTransfer;
using FusionOS.Modules.Inventory.Application.Transfers.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.Modules.Inventory.Domain.Transfers;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Transfers;

public class TransferCommandHandlerTests
{
    [Fact]
    public async Task CreateTransfer_PersistsPendingTransfer()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ITransferRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateTransferCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateTransferCommand(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m),
            CancellationToken.None);

        result.Status.Should().Be("Pending");
        await repository.Received(1).AddAsync(Arg.Any<Transfer>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteTransfer_WithSufficientStock_PostsTwoLedgerEntriesAndCompletes()
    {
        var companyId = Guid.NewGuid();
        var transfer = Transfer.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m);
        var repository = Substitute.For<ITransferRepository>();
        repository.GetByIdAsync(companyId, transfer.Id, Arg.Any<CancellationToken>()).Returns(transfer);
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        ledgerRepository.SumQuantityAsync(companyId, transfer.ProductId, transfer.SourceWarehouseId, Arg.Any<CancellationToken>()).Returns(20m);
        var handler = new CompleteTransferCommandHandler(repository, ledgerRepository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new CompleteTransferCommand(companyId, transfer.Id), CancellationToken.None);

        result.Status.Should().Be("Completed");
        await ledgerRepository.Received(2).AddAsync(Arg.Any<InventoryLedgerEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteTransfer_WithInsufficientStock_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var transfer = Transfer.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m);
        var repository = Substitute.For<ITransferRepository>();
        repository.GetByIdAsync(companyId, transfer.Id, Arg.Any<CancellationToken>()).Returns(transfer);
        var ledgerRepository = Substitute.For<IInventoryLedgerRepository>();
        ledgerRepository.SumQuantityAsync(companyId, transfer.ProductId, transfer.SourceWarehouseId, Arg.Any<CancellationToken>()).Returns(5m);
        var handler = new CompleteTransferCommandHandler(repository, ledgerRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CompleteTransferCommand(companyId, transfer.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CompleteTransfer_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ITransferRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Transfer?)null);
        var handler = new CompleteTransferCommandHandler(repository, Substitute.For<IInventoryLedgerRepository>(), Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CompleteTransferCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CancelTransfer_ResolvesToCancelled()
    {
        var companyId = Guid.NewGuid();
        var transfer = Transfer.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m);
        var repository = Substitute.For<ITransferRepository>();
        repository.GetByIdAsync(companyId, transfer.Id, Arg.Any<CancellationToken>()).Returns(transfer);
        var handler = new CancelTransferCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new CancelTransferCommand(companyId, transfer.Id), CancellationToken.None);

        result.Status.Should().Be("Cancelled");
    }
}
