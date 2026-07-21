using FluentAssertions;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials.Events;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.BillOfMaterials;

public class RoutingOperationTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();

    private static Domain.BillOfMaterials.BillOfMaterials CreateBom() =>
        Domain.BillOfMaterials.BillOfMaterials.Create(Company, "W", "Widget", Product, new[] { new BomLineInput(Guid.NewGuid(), 1m) });

    [Fact]
    public void AddOperation_FirstOperation_GetsSequenceNumberOne()
    {
        var bom = CreateBom();

        var operation = bom.AddOperation("Cut", "Saw-1", 15m);

        operation.SequenceNumber.Should().Be(1);
        bom.Operations.Should().ContainSingle();
        bom.DomainEvents.Should().ContainSingle(e => e is RoutingOperationAdded);
    }

    [Fact]
    public void AddOperation_MultipleOperations_AreSequencedInAppendOrder()
    {
        var bom = CreateBom();

        bom.AddOperation("Cut", "Saw-1", 15m);
        bom.AddOperation("Assemble", "Bench-2", 30m);
        bom.AddOperation("Paint", "Booth-1", 10m);

        bom.Operations.Select(o => o.SequenceNumber).Should().Equal(1, 2, 3);
        bom.Operations.Select(o => o.OperationName).Should().Equal("Cut", "Assemble", "Paint");
    }

    [Fact]
    public void RemoveOperation_RemovesIt_AndLeavesOthersInSequenceOrder()
    {
        var bom = CreateBom();
        var cut = bom.AddOperation("Cut", "Saw-1", 15m);
        bom.AddOperation("Assemble", "Bench-2", 30m);

        bom.RemoveOperation(cut.Id);

        bom.Operations.Should().ContainSingle(o => o.OperationName == "Assemble");
        bom.DomainEvents.Should().ContainSingle(e => e is RoutingOperationRemoved);
    }

    [Fact]
    public void RemoveOperation_UnknownId_Throws()
    {
        var bom = CreateBom();

        var act = () => bom.RemoveOperation(Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReorderOperations_ValidPermutation_ReassignsSequenceNumbers()
    {
        var bom = CreateBom();
        var cut = bom.AddOperation("Cut", "Saw-1", 15m);
        var assemble = bom.AddOperation("Assemble", "Bench-2", 30m);
        var paint = bom.AddOperation("Paint", "Booth-1", 10m);

        bom.ReorderOperations(new[] { paint.Id, cut.Id, assemble.Id });

        bom.Operations.Select(o => o.OperationName).Should().Equal("Paint", "Cut", "Assemble");
        bom.Operations.Select(o => o.SequenceNumber).Should().Equal(1, 2, 3);
        bom.DomainEvents.Should().ContainSingle(e => e is RoutingOperationsReordered);
    }

    [Fact]
    public void ReorderOperations_MissingAnOperation_Throws()
    {
        var bom = CreateBom();
        var cut = bom.AddOperation("Cut", "Saw-1", 15m);
        bom.AddOperation("Assemble", "Bench-2", 30m);

        var act = () => bom.ReorderOperations(new[] { cut.Id });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReorderOperations_WithUnknownId_Throws()
    {
        var bom = CreateBom();
        var cut = bom.AddOperation("Cut", "Saw-1", 15m);
        bom.AddOperation("Assemble", "Bench-2", 30m);

        var act = () => bom.ReorderOperations(new[] { cut.Id, Guid.NewGuid() });

        act.Should().Throw<ArgumentException>();
    }
}
