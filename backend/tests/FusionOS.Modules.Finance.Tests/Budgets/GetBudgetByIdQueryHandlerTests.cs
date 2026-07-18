using FluentAssertions;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Queries.GetBudgetById;
using FusionOS.Modules.Finance.Domain.Budgets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Budgets;

public class GetBudgetByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenBudgetExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var budget = Budget.Create(companyId, "FY2026 Operating Budget", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(365));
        var repository = Substitute.For<IBudgetRepository>();
        repository.GetByIdAsync(companyId, budget.Id, Arg.Any<CancellationToken>()).Returns(budget);
        var handler = new GetBudgetByIdQueryHandler(repository);

        var result = await handler.Handle(new GetBudgetByIdQuery(companyId, budget.Id), CancellationToken.None);

        result.Name.Should().Be("FY2026 Operating Budget");
    }

    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var repository = Substitute.For<IBudgetRepository>();
        repository.GetByIdAsync(companyId, budgetId, Arg.Any<CancellationToken>()).Returns((Budget?)null);
        var handler = new GetBudgetByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetBudgetByIdQuery(companyId, budgetId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
