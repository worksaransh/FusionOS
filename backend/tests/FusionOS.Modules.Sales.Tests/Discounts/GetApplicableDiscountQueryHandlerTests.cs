using FusionOS.Modules.Sales.Application.Discounts.Contracts;
using FusionOS.Modules.Sales.Application.Discounts.Queries.GetApplicableDiscount;
using FusionOS.Modules.Sales.Domain.Discounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Discounts;

/// <summary>Covers the "tiered" half of the tiered discount rules engine (Phase 1 closeout, 2026-07-18) — picking the deepest matching tier.</summary>
public class GetApplicableDiscountQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithQuantityMeetingTheHighestTier_ReturnsThatTiersDiscount()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var tier1 = DiscountRule.Create(companyId, productId, 10m, 5m);
        var tier2 = DiscountRule.Create(companyId, productId, 50m, 10m);
        var tier3 = DiscountRule.Create(companyId, productId, 100m, 15m);
        var repository = Substitute.For<IDiscountRuleRepository>();
        repository.ListActiveForProductAsync(companyId, productId, Arg.Any<CancellationToken>())
            .Returns(new List<DiscountRule> { tier1, tier2, tier3 });
        var handler = new GetApplicableDiscountQueryHandler(repository);

        var result = await handler.Handle(new GetApplicableDiscountQuery(companyId, productId, 120m), CancellationToken.None);

        result.DiscountRuleId.Should().Be(tier3.Id);
        result.DiscountPercentage.Should().Be(15m);
    }

    [Fact]
    public async Task Handle_WithQuantityBetweenTiers_ReturnsTheHighestTierItMeets()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var tier1 = DiscountRule.Create(companyId, productId, 10m, 5m);
        var tier2 = DiscountRule.Create(companyId, productId, 50m, 10m);
        var tier3 = DiscountRule.Create(companyId, productId, 100m, 15m);
        var repository = Substitute.For<IDiscountRuleRepository>();
        repository.ListActiveForProductAsync(companyId, productId, Arg.Any<CancellationToken>())
            .Returns(new List<DiscountRule> { tier1, tier2, tier3 });
        var handler = new GetApplicableDiscountQueryHandler(repository);

        var result = await handler.Handle(new GetApplicableDiscountQuery(companyId, productId, 75m), CancellationToken.None);

        result.DiscountRuleId.Should().Be(tier2.Id);
        result.DiscountPercentage.Should().Be(10m);
    }

    [Fact]
    public async Task Handle_WithQuantityBelowEveryTier_ReturnsNoDiscount()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var tier1 = DiscountRule.Create(companyId, productId, 10m, 5m);
        var repository = Substitute.For<IDiscountRuleRepository>();
        repository.ListActiveForProductAsync(companyId, productId, Arg.Any<CancellationToken>())
            .Returns(new List<DiscountRule> { tier1 });
        var handler = new GetApplicableDiscountQueryHandler(repository);

        var result = await handler.Handle(new GetApplicableDiscountQuery(companyId, productId, 5m), CancellationToken.None);

        result.DiscountRuleId.Should().BeNull();
        result.DiscountPercentage.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WithNoRulesForProduct_ReturnsNoDiscount()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IDiscountRuleRepository>();
        repository.ListActiveForProductAsync(companyId, productId, Arg.Any<CancellationToken>())
            .Returns(new List<DiscountRule>());
        var handler = new GetApplicableDiscountQueryHandler(repository);

        var result = await handler.Handle(new GetApplicableDiscountQuery(companyId, productId, 1000m), CancellationToken.None);

        result.DiscountRuleId.Should().BeNull();
        result.DiscountPercentage.Should().Be(0m);
    }
}
