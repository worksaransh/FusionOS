using FluentAssertions;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using FusionOS.Modules.Finance.Application.BudgetLines.Queries.ListBudgetLines;
using FusionOS.Modules.Finance.Domain.BudgetLines;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BudgetLines;

public class ListBudgetLinesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedBudgetLinesForTheBudget()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var budgetLines = new[] { BudgetLine.Create(companyId, budgetId, Guid.NewGuid(), null, 500m, null) };
        var repository = Substitute.For<IBudgetLineRepository>();
        repository.ListByBudgetAsync(companyId, budgetId, 1, 25, Arg.Any<CancellationToken>()).Returns(budgetLines);
        repository.CountByBudgetAsync(companyId, budgetId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListBudgetLinesQueryHandler(repository);

        var result = await handler.Handle(new ListBudgetLinesQuery(companyId, budgetId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(l => l.BudgetId == budgetId && l.BudgetedAmount == 500m);
    }
}
