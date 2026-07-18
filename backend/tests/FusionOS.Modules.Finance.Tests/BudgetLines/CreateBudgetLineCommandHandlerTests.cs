using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Application.BudgetLines.Commands.CreateBudgetLine;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Domain.CostCenters;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BudgetLines;

public class CreateBudgetLineCommandHandlerTests
{
    private static (IBudgetLineRepository repo, IBudgetRepository budgetRepo, IAccountRepository accountRepo, ICostCenterRepository costCenterRepo, IUnitOfWork uow) MakeSubstitutes(Guid companyId, Guid budgetId, Guid accountId)
    {
        var repo = Substitute.For<IBudgetLineRepository>();
        var budgetRepo = Substitute.For<IBudgetRepository>();
        budgetRepo.ExistsAsync(companyId, budgetId, Arg.Any<CancellationToken>()).Returns(true);
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.ExistsAsync(companyId, accountId, Arg.Any<CancellationToken>()).Returns(true);
        var costCenterRepo = Substitute.For<ICostCenterRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        return (repo, budgetRepo, accountRepo, costCenterRepo, uow);
    }

    [Fact]
    public async Task Handle_WhenBudgetAndAccountExist_PersistsBudgetLine()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var (repo, budgetRepo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, budgetId, accountId);
        var handler = new CreateBudgetLineCommandHandler(repo, budgetRepo, accountRepo, costCenterRepo, uow);
        var command = new CreateBudgetLineCommand(companyId, budgetId, accountId, null, 500m, "Notes");

        var result = await handler.Handle(command, CancellationToken.None);

        result.BudgetId.Should().Be(budgetId);
        result.AccountId.Should().Be(accountId);
        result.BudgetedAmount.Should().Be(500m);
        await repo.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.BudgetLines.BudgetLine>(), Arg.Any<CancellationToken>());
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var (repo, budgetRepo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, budgetId, accountId);
        budgetRepo.ExistsAsync(companyId, budgetId, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateBudgetLineCommandHandler(repo, budgetRepo, accountRepo, costCenterRepo, uow);
        var command = new CreateBudgetLineCommand(companyId, budgetId, accountId, null, 500m, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var (repo, budgetRepo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, budgetId, accountId);
        accountRepo.ExistsAsync(companyId, accountId, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateBudgetLineCommandHandler(repo, budgetRepo, accountRepo, costCenterRepo, uow);
        var command = new CreateBudgetLineCommand(companyId, budgetId, accountId, null, 500m, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCostCenterDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var costCenterId = Guid.NewGuid();
        var (repo, budgetRepo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, budgetId, accountId);
        costCenterRepo.GetByIdAsync(companyId, costCenterId, Arg.Any<CancellationToken>()).Returns((CostCenter?)null);
        var handler = new CreateBudgetLineCommandHandler(repo, budgetRepo, accountRepo, costCenterRepo, uow);
        var command = new CreateBudgetLineCommand(companyId, budgetId, accountId, costCenterId, 500m, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
