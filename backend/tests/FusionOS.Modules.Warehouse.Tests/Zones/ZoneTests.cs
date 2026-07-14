using FusionOS.Modules.Warehouse.Domain.Zones;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Zones;

public class ZoneTests
{
    [Fact]
    public void Create_WithValidData_NormalizesCode()
    {
        var zone = Zone.Create(Guid.NewGuid(), Guid.NewGuid(), "Receiving Dock", " z-01 ");

        zone.Code.Should().Be("Z-01");
        zone.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutWarehouseId_Throws()
    {
        var act = () => Zone.Create(Guid.NewGuid(), Guid.Empty, "Receiving Dock", "Z-01");

        act.Should().Throw<ArgumentException>();
    }
}
