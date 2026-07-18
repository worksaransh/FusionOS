using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Budgets;

public class CreateBudgetCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_PersistsBudget()
    {
        var repository = Substitute.For<IBudgetRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBudgetCommandHandler(repository, unitOfWork);
        var command = new CreateBudgetCommand(Guid.NewGuid(), "FY2026 Operating Budget", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(365));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("FY2026 Operating Budget");
        result.IsActive.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.Budgets.Budget>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
