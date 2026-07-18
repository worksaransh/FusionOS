using FluentAssertions;
using FusionOS.Modules.Marketplace.Domain.PluginListings;
using FusionOS.Modules.Marketplace.Domain.PluginListings.Events;
using Xunit;

namespace FusionOS.Modules.Marketplace.Tests.PluginListings;

public class PluginListingTests
{
    private static readonly Guid Company = Guid.NewGuid();

    [Fact]
    public void Create_WithValidFields_RaisesCreatedEvent()
    {
        var listing = PluginListing.Create(Company, "wh-scan", "Warehouse Scanner", "Acme Co", PluginCategory.Plugin);

        listing.Code.Should().Be("WH-SCAN");
        listing.Category.Should().Be(PluginCategory.Plugin);
        listing.IsActive.Should().BeTrue();
        listing.DomainEvents.Should().ContainSingle(e => e is PluginListingCreated);
    }

    [Fact]
    public void Create_WithBlankPublisher_Throws()
    {
        var act = () => PluginListing.Create(Company, "WH-SCAN", "Warehouse Scanner", "  ", PluginCategory.Plugin);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var listing = PluginListing.Create(Company, "WH-SCAN", "Warehouse Scanner", "Acme Co", PluginCategory.Theme);

        listing.Deactivate();

        listing.IsActive.Should().BeFalse();
    }
}
