using FusionOS.Modules.Sales.Domain.PriceLists;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.PriceLists;

public class PriceListTests
{
    private static readonly PriceListEntryInput[] OneEntry =
    {
        new(Guid.NewGuid(), 42m),
    };

    [Fact]
    public void Create_WithValidEntries_StartsActive()
    {
        var priceList = PriceList.Create(Guid.NewGuid(), "Wholesale", OneEntry);

        priceList.IsActive.Should().BeTrue();
        priceList.Entries.Should().ContainSingle(e => e.UnitPrice == 42m);
        priceList.DomainEvents.Should().ContainSingle(e => e is Events.PriceListCreated);
    }

    [Fact]
    public void Create_WithNoEntries_Throws()
    {
        var act = () => PriceList.Create(Guid.NewGuid(), "Wholesale", Array.Empty<PriceListEntryInput>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithBlankName_Throws()
    {
        var act = () => PriceList.Create(Guid.NewGuid(), "  ", OneEntry);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var priceList = PriceList.Create(Guid.NewGuid(), "Wholesale", OneEntry);

        priceList.Deactivate();

        priceList.IsActive.Should().BeFalse();
    }
}
