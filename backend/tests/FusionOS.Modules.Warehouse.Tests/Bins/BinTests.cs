using FusionOS.Modules.Warehouse.Domain.Bins;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Bins;

public class BinTests
{
    [Fact]
    public void Create_WithValidData_NormalizesCode()
    {
        var bin = Bin.Create(Guid.NewGuid(), Guid.NewGuid(), "Shelf 3", " a-01-03 ");

        bin.Code.Should().Be("A-01-03");
        bin.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutZoneId_Throws()
    {
        var act = () => Bin.Create(Guid.NewGuid(), Guid.Empty, "Shelf 3", "A-01-03");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var bin = Bin.Create(Guid.NewGuid(), Guid.NewGuid(), "Shelf 3", "A-01-03");

        bin.Deactivate();

        bin.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_ChangesName()
    {
        var bin = Bin.Create(Guid.NewGuid(), Guid.NewGuid(), "Shelf 3", "A-01-03");

        bin.UpdateDetails("Shelf 3 — Top");

        bin.Name.Should().Be("Shelf 3 — Top");
        bin.Code.Should().Be("A-01-03");
    }
}
