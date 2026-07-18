using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Commands.UpdateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Domain.Budgets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Budgets;

public class UpdateBudgetCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBudgetExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var budget = Budget.Create(companyId, "Old Name", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        var repository = Substitute.For<IBudgetRepository>();
        repository.GetByIdAsync(companyId, budget.Id, Arg.Any<CancellationToken>()).Returns(budget);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateBudgetCommandHandler(repository, unitOfWork);
        var newStart = DateTimeOffset.UtcNow.AddDays(1);
        var newEnd = newStart.AddDays(90);
        var command = new UpdateBudgetCommand(companyId, budget.Id, "New Name", newStart, newEnd);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.PeriodStart.Should().Be(newStart);
        result.PeriodEnd.Should().Be(newEnd);
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
        var handler = new UpdateBudgetCommandHandler(repository, unitOfWork);
        var command = new UpdateBudgetCommand(companyId, budgetId, "Name", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
