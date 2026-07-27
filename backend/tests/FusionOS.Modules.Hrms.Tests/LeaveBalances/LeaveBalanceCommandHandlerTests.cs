using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.AdjustLeaveBalance;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using FusionOS.Modules.Hrms.Domain.LeaveBalances;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.LeaveBalances;

/// <summary>
/// AdjustLeaveBalanceCommandHandler had zero test coverage. Its one piece of real logic is
/// translating LeaveBalance.AdjustEntitlement's InvalidOperationException (entitlement set
/// below days already used) into a FluentValidation ValidationException — a re-throw
/// convention shared with UpdateRoleCommandHandler per the handler's own comment.
/// </summary>
public class LeaveBalanceCommandHandlerTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Employee = Guid.NewGuid();

    private static LeaveBalance CreateBalanceWithUsedDays(decimal entitled, decimal used)
    {
        var balance = LeaveBalance.Create(Company, Employee, LeaveType.Annual, 2026, entitled);
        if (used > 0)
            balance.Deduct(used);
        return balance;
    }

    [Fact]
    public async Task AdjustLeaveBalance_IncreasingEntitlement_UpdatesEntitledDays()
    {
        var balance = CreateBalanceWithUsedDays(entitled: 12m, used: 5m);
        var repository = Substitute.For<ILeaveBalanceRepository>();
        repository.GetByIdAsync(Company, balance.Id, Arg.Any<CancellationToken>()).Returns(balance);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AdjustLeaveBalanceCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new AdjustLeaveBalanceCommand(Company, balance.Id, 15m), CancellationToken.None);

        result.EntitledDays.Should().Be(15m);
        result.UsedDays.Should().Be(5m);
        result.RemainingDays.Should().Be(10m);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustLeaveBalance_BelowUsedDays_ThrowsValidationExceptionNotDomainException()
    {
        var balance = CreateBalanceWithUsedDays(entitled: 12m, used: 8m);
        var repository = Substitute.For<ILeaveBalanceRepository>();
        repository.GetByIdAsync(Company, balance.Id, Arg.Any<CancellationToken>()).Returns(balance);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AdjustLeaveBalanceCommandHandler(repository, unitOfWork);

        // 8 days already used; setting entitlement to 5 would make RemainingDays negative —
        // the domain rejects this with InvalidOperationException, which the handler must
        // translate into a ValidationException (not let the raw domain exception surface).
        var act = () => handler.Handle(new AdjustLeaveBalanceCommand(Company, balance.Id, 5m), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustLeaveBalance_ExactlyAtUsedDays_Succeeds()
    {
        // Boundary case: newEntitledDays == UsedDays is allowed (RemainingDays becomes exactly 0).
        var balance = CreateBalanceWithUsedDays(entitled: 12m, used: 8m);
        var repository = Substitute.For<ILeaveBalanceRepository>();
        repository.GetByIdAsync(Company, balance.Id, Arg.Any<CancellationToken>()).Returns(balance);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AdjustLeaveBalanceCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new AdjustLeaveBalanceCommand(Company, balance.Id, 8m), CancellationToken.None);

        result.EntitledDays.Should().Be(8m);
        result.RemainingDays.Should().Be(0m);
    }

    [Fact]
    public async Task AdjustLeaveBalance_WhenMissing_ThrowsKeyNotFound()
    {
        var repository = Substitute.For<ILeaveBalanceRepository>();
        repository.GetByIdAsync(Company, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LeaveBalance?)null);
        var handler = new AdjustLeaveBalanceCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new AdjustLeaveBalanceCommand(Company, Guid.NewGuid(), 10m), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
