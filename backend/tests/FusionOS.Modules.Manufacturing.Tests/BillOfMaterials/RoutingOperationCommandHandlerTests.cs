using FluentAssertions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.AddRoutingOperation;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.RemoveRoutingOperation;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.ReorderRoutingOperations;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.BillOfMaterials;

public class RoutingOperationCommandHandlerTests
{
    private static Domain.BillOfMaterials.BillOfMaterials BuildBom(Guid companyId) =>
        Domain.BillOfMaterials.BillOfMaterials.Create(companyId, "WIDGET-A", "Widget A", Guid.NewGuid(), new[] { new BomLineInput(Guid.NewGuid(), 1m) });

    [Fact]
    public async Task AddRoutingOperation_AppendsOperation_AndSaves()
    {
        var companyId = Guid.NewGuid();
        var bom = BuildBom(companyId);
        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.GetByIdAsync(companyId, bom.Id, Arg.Any<CancellationToken>()).Returns(bom);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AddRoutingOperationCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new AddRoutingOperationCommand(companyId, bom.Id, "Cut", "Saw-1", 15m), CancellationToken.None);

        result.Operations.Should().ContainSingle(o => o.OperationName == "Cut" && o.SequenceNumber == 1);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddRoutingOperation_WhenBomMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.BillOfMaterials.BillOfMaterials?)null);
        var handler = new AddRoutingOperationCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new AddRoutingOperationCommand(companyId, Guid.NewGuid(), "Cut", "Saw-1", 15m), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task RemoveRoutingOperation_RemovesIt_AndSaves()
    {
        var companyId = Guid.NewGuid();
        var bom = BuildBom(companyId);
        var operation = bom.AddOperation("Cut", "Saw-1", 15m);
        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.GetByIdAsync(companyId, bom.Id, Arg.Any<CancellationToken>()).Returns(bom);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RemoveRoutingOperationCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new RemoveRoutingOperationCommand(companyId, bom.Id, operation.Id), CancellationToken.None);

        result.Operations.Should().BeEmpty();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReorderRoutingOperations_ReassignsSequence_AndSaves()
    {
        var companyId = Guid.NewGuid();
        var bom = BuildBom(companyId);
        var cut = bom.AddOperation("Cut", "Saw-1", 15m);
        var assemble = bom.AddOperation("Assemble", "Bench-2", 30m);
        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.GetByIdAsync(companyId, bom.Id, Arg.Any<CancellationToken>()).Returns(bom);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ReorderRoutingOperationsCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ReorderRoutingOperationsCommand(companyId, bom.Id, new[] { assemble.Id, cut.Id }), CancellationToken.None);

        result.Operations.Select(o => o.OperationName).Should().Equal("Assemble", "Cut");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
