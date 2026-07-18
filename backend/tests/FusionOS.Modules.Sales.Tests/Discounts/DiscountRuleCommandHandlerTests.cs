using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Discounts.Commands.CreateDiscountRule;
using FusionOS.Modules.Sales.Application.Discounts.Commands.DeactivateDiscountRule;
using FusionOS.Modules.Sales.Application.Discounts.Contracts;
using FusionOS.Modules.Sales.Domain.Discounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Discounts;

public class DiscountRuleCommandHandlerTests
{
    [Fact]
    public async Task CreateDiscountRule_PersistsActiveRule()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IDiscountRuleRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDiscountRuleCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateDiscountRuleCommand(companyId, Guid.NewGuid(), 50m, 10m),
            CancellationToken.None);

        result.IsActive.Should().BeTrue();
        result.MinQuantity.Should().Be(50m);
        result.DiscountPercentage.Should().Be(10m);
        await repository.Received(1).AddAsync(Arg.Any<DiscountRule>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateDiscountRule_ResolvesToInactive()
    {
        var companyId = Guid.NewGuid();
        var rule = DiscountRule.Create(companyId, Guid.NewGuid(), 50m, 10m);
        var repository = Substitute.For<IDiscountRuleRepository>();
        repository.GetByIdAsync(companyId, rule.Id, Arg.Any<CancellationToken>()).Returns(rule);
        var handler = new DeactivateDiscountRuleCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new DeactivateDiscountRuleCommand(companyId, rule.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateDiscountRule_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IDiscountRuleRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DiscountRule?)null);
        var handler = new DeactivateDiscountRuleCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateDiscountRuleCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
