using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.SuggestPutawayBin;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Bins;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using FluentAssertions;
using NSubstitute;
using Xunit;
using GoodsReceiptEntity = FusionOS.Modules.Warehouse.Domain.GoodsReceipts.GoodsReceipt;

namespace FusionOS.Modules.Warehouse.Tests.GoodsReceipts;

public class SuggestPutawayBinCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenActiveBinExistsInZone_SuggestsItAndSaves()
    {
        var companyId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var receipt = GoodsReceiptEntity.Create(companyId, Guid.NewGuid(), zoneId, null, null,
            new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 10m, null) });
        var bin = Bin.Create(companyId, zoneId, "Bin A", "A1");

        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.GetByIdAsync(receipt.Id, Arg.Any<CancellationToken>()).Returns(receipt);
        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetFirstActiveBinAsync(companyId, zoneId, Arg.Any<CancellationToken>()).Returns(bin);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SuggestPutawayBinCommandHandler(repository, binRepository, unitOfWork);
        var command = new SuggestPutawayBinCommand(companyId, receipt.Id, receipt.Lines[0].Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Lines.Should().ContainSingle(l => l.SuggestedBinId == bin.Id);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReceiptNotFound_Throws()
    {
        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((GoodsReceiptEntity?)null);
        var binRepository = Substitute.For<IBinRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SuggestPutawayBinCommandHandler(repository, binRepository, unitOfWork);
        var command = new SuggestPutawayBinCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNoActiveBinInZone_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var receipt = GoodsReceiptEntity.Create(companyId, Guid.NewGuid(), zoneId, null, null,
            new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 10m, null) });

        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.GetByIdAsync(receipt.Id, Arg.Any<CancellationToken>()).Returns(receipt);
        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetFirstActiveBinAsync(companyId, zoneId, Arg.Any<CancellationToken>()).Returns((Bin?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SuggestPutawayBinCommandHandler(repository, binRepository, unitOfWork);
        var command = new SuggestPutawayBinCommand(companyId, receipt.Id, receipt.Lines[0].Id);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
