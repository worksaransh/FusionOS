using FusionOS.Modules.Sales.Domain.Commissions;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Commissions;

public class SalesCommissionRateTests
{
    [Fact]
    public void Create_WithValidArgs_SetsRate()
    {
        var userId = Guid.NewGuid();

        var rate = SalesCommissionRate.Create(Guid.NewGuid(), userId, 5m);

        rate.UserId.Should().Be(userId);
        rate.RatePercentage.Should().Be(5m);
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        var act = () => SalesCommissionRate.Create(Guid.NewGuid(), Guid.Empty, 5m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_WithOutOfRangeRate_Throws(decimal rate)
    {
        var act = () => SalesCommissionRate.Create(Guid.NewGuid(), Guid.NewGuid(), rate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetRate_WithValidValue_Overwrites()
    {
        var rate = SalesCommissionRate.Create(Guid.NewGuid(), Guid.NewGuid(), 5m);

        rate.SetRate(10m);

        rate.RatePercentage.Should().Be(10m);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void SetRate_WithOutOfRangeRate_Throws(decimal rate)
    {
        var commissionRate = SalesCommissionRate.Create(Guid.NewGuid(), Guid.NewGuid(), 5m);

        var act = () => commissionRate.SetRate(rate);

        act.Should().Throw<ArgumentException>();
    }
}
