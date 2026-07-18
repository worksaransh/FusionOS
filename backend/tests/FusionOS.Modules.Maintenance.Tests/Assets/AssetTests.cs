using FluentAssertions;
using FusionOS.Modules.Maintenance.Domain.Assets;
using FusionOS.Modules.Maintenance.Domain.Assets.Events;
using Xunit;

namespace FusionOS.Modules.Maintenance.Tests.Assets;

public class AssetTests
{
    private static readonly Guid Company = Guid.NewGuid();

    [Fact]
    public void Create_WithValidFields_RaisesCreatedEvent()
    {
        var asset = Asset.Create(Company, "gen-01", "Generator 1", "Plant A, Bay 3");

        asset.Code.Should().Be("GEN-01");
        asset.Name.Should().Be("Generator 1");
        asset.Location.Should().Be("Plant A, Bay 3");
        asset.IsActive.Should().BeTrue();
        asset.DomainEvents.Should().ContainSingle(e => e is AssetCreated);
    }

    [Fact]
    public void Create_WithNoLocation_LeavesLocationNull()
    {
        var asset = Asset.Create(Company, "gen-02", "Generator 2", null);

        asset.Location.Should().BeNull();
    }

    [Fact]
    public void Create_WithBlankCode_Throws()
    {
        var act = () => Asset.Create(Company, "  ", "Generator 1", null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var asset = Asset.Create(Company, "GEN-01", "Generator 1", null);

        asset.Deactivate();

        asset.IsActive.Should().BeFalse();
    }
}
