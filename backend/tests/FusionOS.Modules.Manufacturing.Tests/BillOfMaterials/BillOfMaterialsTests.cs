using FluentAssertions;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials.Events;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.BillOfMaterials;

public class BillOfMaterialsTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();

    private static BomLineInput Line() => new(Guid.NewGuid(), 2m);

    [Fact]
    public void Create_NormalizesCode_AddsLines_AndRaisesEvent()
    {
        var bom = Domain.BillOfMaterials.BillOfMaterials.Create(Company, " widget-a ", "Widget A", Product, new[] { Line(), Line() });

        bom.Code.Should().Be("WIDGET-A");
        bom.ProductId.Should().Be(Product);
        bom.IsActive.Should().BeTrue();
        bom.Lines.Should().HaveCount(2);
        bom.DomainEvents.Should().ContainSingle(e => e is BillOfMaterialsCreated);
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        var act = () => Domain.BillOfMaterials.BillOfMaterials.Create(Company, "W", "Widget", Product, Array.Empty<BomLineInput>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ComponentEqualToParentProduct_Throws()
    {
        var act = () => Domain.BillOfMaterials.BillOfMaterials.Create(Company, "W", "Widget", Product, new[] { new BomLineInput(Product, 1m) });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_DuplicateComponent_Throws()
    {
        var component = Guid.NewGuid();
        var act = () => Domain.BillOfMaterials.BillOfMaterials.Create(Company, "W", "Widget", Product,
            new[] { new BomLineInput(component, 1m), new BomLineInput(component, 3m) });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsInactive()
    {
        var bom = Domain.BillOfMaterials.BillOfMaterials.Create(Company, "W", "Widget", Product, new[] { Line() });

        bom.Deactivate();

        bom.IsActive.Should().BeFalse();
    }
}
