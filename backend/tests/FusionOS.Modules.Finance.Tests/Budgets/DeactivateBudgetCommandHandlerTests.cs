using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Commands.DeactivateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Domain.Budgets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Budgets;

public class DeactivateBudgetCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBudgetExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var budget = Budget.Create(companyId, "Budget", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        var repository = Substitute.For<IBudgetRepository>();
        repository.GetByIdAsync(companyId, budget.Id, Arg.Any<CancellationToken>()).Returns(budget);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateBudgetCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateBudgetCommand(companyId, budget.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var repository = Substitute.For<IBudgetRepository>();
        repository.GetByIdAsync(companyId, budgetId, Arg.Any<CancellationToken>()).Returns((Budget?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateBudgetCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateBudgetCommand(companyId, budgetId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
