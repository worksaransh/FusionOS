using WarehouseEntity = FusionOS.Modules.Warehouse.Domain.Warehouses.Warehouse;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Warehouses;

public class WarehouseTests
{
    [Fact]
    public void Create_WithValidData_NormalizesCode()
    {
        var warehouse = WarehouseEntity.Create(Guid.NewGuid(), null, "Main DC", " wh-01 ");

        warehouse.Code.Should().Be("WH-01");
        warehouse.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutCode_Throws(string invalidCode)
    {
        var act = () => WarehouseEntity.Create(Guid.NewGuid(), null, "Main DC", invalidCode);

        act.Should().Throw<ArgumentException>();
    }
}
