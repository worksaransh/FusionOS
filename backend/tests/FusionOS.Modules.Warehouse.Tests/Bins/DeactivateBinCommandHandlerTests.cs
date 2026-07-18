using FusionOS.Modules.Warehouse.Application.Bins.Commands.DeactivateBin;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Bins;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Bins;

public class DeactivateBinCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBinExists_Deactivates()
    {
        var companyId = Guid.NewGuid();
        var bin = Bin.Create(companyId, Guid.NewGuid(), "Shelf 3", "A-01-03");
        var repository = Substitute.For<IBinRepository>();
        repository.GetByIdAsync(bin.Id, Arg.Any<CancellationToken>()).Returns(bin);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateBinCommandHandler(repository, unitOfWork);
        var command = new DeactivateBinCommand(companyId, bin.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBinBelongsToDifferentCompany_Throws()
    {
        var bin = Bin.Create(Guid.NewGuid(), Guid.NewGuid(), "Shelf 3", "A-01-03");
        var repository = Substitute.For<IBinRepository>();
        repository.GetByIdAsync(bin.Id, Arg.Any<CancellationToken>()).Returns(bin);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateBinCommandHandler(repository, unitOfWork);
        var command = new DeactivateBinCommand(Guid.NewGuid(), bin.Id);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
