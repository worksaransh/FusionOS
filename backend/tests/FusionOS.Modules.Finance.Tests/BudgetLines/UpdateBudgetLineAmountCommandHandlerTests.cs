using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BudgetLines.Commands.UpdateBudgetLineAmount;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using FusionOS.Modules.Finance.Domain.BudgetLines;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BudgetLines;

public class UpdateBudgetLineAmountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBudgetLineExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var budgetLine = BudgetLine.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), null, 100m, "Original");
        var repository = Substitute.For<IBudgetLineRepository>();
        repository.GetByIdAsync(companyId, budgetLine.Id, Arg.Any<CancellationToken>()).Returns(budgetLine);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateBudgetLineAmountCommandHandler(repository, unitOfWork);
        var command = new UpdateBudgetLineAmountCommand(companyId, budgetLine.Id, 300m, "Corrected");

        var result = await handler.Handle(command, CancellationToken.None);

        result.BudgetedAmount.Should().Be(300m);
        result.Notes.Should().Be("Corrected");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBudgetLineDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var budgetLineId = Guid.NewGuid();
        var repository = Substitute.For<IBudgetLineRepository>();
        repository.GetByIdAsync(companyId, budgetLineId, Arg.Any<CancellationToken>()).Returns((BudgetLine?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateBudgetLineAmountCommandHandler(repository, unitOfWork);
        var command = new UpdateBudgetLineAmountCommand(companyId, budgetLineId, 300m, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
