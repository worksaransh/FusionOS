using FusionOS.Modules.Warehouse.Domain.Shelves;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Shelves;

public class ShelfTests
{
    [Fact]
    public void Create_WithValidData_NormalizesCode()
    {
        var shelf = Shelf.Create(Guid.NewGuid(), Guid.NewGuid(), "Top Shelf", " s-01 ");

        shelf.Code.Should().Be("S-01");
        shelf.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutRackId_Throws()
    {
        var act = () => Shelf.Create(Guid.NewGuid(), Guid.Empty, "Top Shelf", "S-01");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var shelf = Shelf.Create(Guid.NewGuid(), Guid.NewGuid(), "Top Shelf", "S-01");

        shelf.Deactivate();

        shelf.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_ChangesName()
    {
        var shelf = Shelf.Create(Guid.NewGuid(), Guid.NewGuid(), "Top Shelf", "S-01");

        shelf.UpdateDetails("Top Shelf — Wide");

        shelf.Name.Should().Be("Top Shelf — Wide");
        shelf.Code.Should().Be("S-01");
    }
}
