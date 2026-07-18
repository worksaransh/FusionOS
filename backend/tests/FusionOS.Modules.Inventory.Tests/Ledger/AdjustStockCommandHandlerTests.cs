using FusionOS.Modules.Inventory.Application.Ledger.Commands.AdjustStock;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Ledger;

public class AdjustStockCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsLedgerEntry()
    {
        var repository = Substitute.For<IInventoryLedgerRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AdjustStockCommandHandler(repository, unitOfWork);
        var command = new AdjustStockCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -5m, "Damaged in transit", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.QuantityDelta.Should().Be(-5m);
        await repository.Received(1).AddAsync(Arg.Any<InventoryLedgerEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithBatchAndSerialNumber_PassesThemThroughToTheLedgerEntryAndDto()
    {
        var repository = Substitute.For<IInventoryLedgerRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AdjustStockCommandHandler(repository, unitOfWork);
        var command = new AdjustStockCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, "Goods receipt", 5m, "LOT-42", "SN-001");

        var result = await handler.Handle(command, CancellationToken.None);

        result.BatchNumber.Should().Be("LOT-42");
        result.SerialNumber.Should().Be("SN-001");
    }
}
