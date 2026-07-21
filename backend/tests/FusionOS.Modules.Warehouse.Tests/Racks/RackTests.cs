using FusionOS.Modules.Warehouse.Domain.Racks;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Racks;

public class RackTests
{
    [Fact]
    public void Create_WithValidData_NormalizesCode()
    {
        var rack = Rack.Create(Guid.NewGuid(), Guid.NewGuid(), "Aisle 3", " r-01 ");

        rack.Code.Should().Be("R-01");
        rack.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutZoneId_Throws()
    {
        var act = () => Rack.Create(Guid.NewGuid(), Guid.Empty, "Aisle 3", "R-01");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var rack = Rack.Create(Guid.NewGuid(), Guid.NewGuid(), "Aisle 3", "R-01");

        rack.Deactivate();

        rack.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_ChangesName()
    {
        var rack = Rack.Create(Guid.NewGuid(), Guid.NewGuid(), "Aisle 3", "R-01");

        rack.UpdateDetails("Aisle 3 — Tall");

        rack.Name.Should().Be("Aisle 3 — Tall");
        rack.Code.Should().Be("R-01");
    }
}
