using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.ConfirmPutaway;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Bins;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using FluentAssertions;
using NSubstitute;
using Xunit;
using GoodsReceiptEntity = FusionOS.Modules.Warehouse.Domain.GoodsReceipts.GoodsReceipt;

namespace FusionOS.Modules.Warehouse.Tests.GoodsReceipts;

public class ConfirmPutawayCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBinBelongsToReceiptZone_ConfirmsAndSaves()
    {
        var companyId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var receipt = GoodsReceiptEntity.Create(companyId, Guid.NewGuid(), zoneId, null, null,
            new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 10m, null) });
        var bin = Bin.Create(companyId, zoneId, "Bin A", "A1");

        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.GetByIdAsync(receipt.Id, Arg.Any<CancellationToken>()).Returns(receipt);
        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetByIdAsync(bin.Id, Arg.Any<CancellationToken>()).Returns(bin);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConfirmPutawayCommandHandler(repository, binRepository, unitOfWork);
        var command = new ConfirmPutawayCommand(companyId, receipt.Id, receipt.Lines[0].Id, bin.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Lines.Should().ContainSingle(l => l.PutAwayBinId == bin.Id && l.IsPutAway);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReceiptNotFound_Throws()
    {
        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((GoodsReceiptEntity?)null);
        var binRepository = Substitute.For<IBinRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConfirmPutawayCommandHandler(repository, binRepository, unitOfWork);
        var command = new ConfirmPutawayCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenBinBelongsToADifferentZone_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var receiptZoneId = Guid.NewGuid();
        var otherZoneId = Guid.NewGuid();
        var receipt = GoodsReceiptEntity.Create(companyId, Guid.NewGuid(), receiptZoneId, null, null,
            new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 10m, null) });
        var binInOtherZone = Bin.Create(companyId, otherZoneId, "Bin B", "B1");

        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.GetByIdAsync(receipt.Id, Arg.Any<CancellationToken>()).Returns(receipt);
        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetByIdAsync(binInOtherZone.Id, Arg.Any<CancellationToken>()).Returns(binInOtherZone);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConfirmPutawayCommandHandler(repository, binRepository, unitOfWork);
        var command = new ConfirmPutawayCommand(companyId, receipt.Id, receipt.Lines[0].Id, binInOtherZone.Id);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBinDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var receipt = GoodsReceiptEntity.Create(companyId, Guid.NewGuid(), zoneId, null, null,
            new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 10m, null) });

        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.GetByIdAsync(receipt.Id, Arg.Any<CancellationToken>()).Returns(receipt);
        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Bin?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConfirmPutawayCommandHandler(repository, binRepository, unitOfWork);
        var command = new ConfirmPutawayCommand(companyId, receipt.Id, receipt.Lines[0].Id, Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
