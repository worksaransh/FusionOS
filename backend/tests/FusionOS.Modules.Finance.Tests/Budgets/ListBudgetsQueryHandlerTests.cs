using FluentAssertions;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Queries.ListBudgets;
using FusionOS.Modules.Finance.Domain.Budgets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Budgets;

public class ListBudgetsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedBudgetsForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var budgets = new[] { Budget.Create(companyId, "FY2026 Operating Budget", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(365)) };
        var repository = Substitute.For<IBudgetRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(budgets);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListBudgetsQueryHandler(repository);

        var result = await handler.Handle(new ListBudgetsQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(b => b.Name == "FY2026 Operating Budget");
    }
}
