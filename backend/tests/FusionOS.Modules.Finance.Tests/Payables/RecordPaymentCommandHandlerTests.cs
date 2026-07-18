using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Commands.RecordPayment;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Domain.Payables;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Payables;

/// <summary>
/// Covers Payables' RecordPaymentCommand (Phase M8c, 2026-07-17) — including
/// the overpayment guard, scoped to the supplier's total outstanding balance
/// (SumAmountAsync) rather than one specific invoice/PO the way AR's
/// equivalent test class guards per-invoice — see ApLedgerEntry's class doc
/// comment for why.
/// </summary>
public class RecordPaymentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPaymentIsWithinOutstandingBalance_PersistsANegativeLedgerEntry()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns(250m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPaymentCommandHandler(repository, unitOfWork);
        var command = new RecordPaymentCommand(companyId, supplierId, purchaseOrderId, 100m, PaymentDate: null, Reference: "WIRE1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(-100m);
        result.SupplierId.Should().Be(supplierId);
        await repository.Received(1).AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPaymentExceedsOutstandingBalance_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns(50m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPaymentCommandHandler(repository, unitOfWork);
        var command = new RecordPaymentCommand(companyId, supplierId, null, 100m, PaymentDate: null, Reference: null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSupplierHasNoChargesYet_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPaymentCommandHandler(repository, unitOfWork);
        var command = new RecordPaymentCommand(companyId, supplierId, null, 1m, PaymentDate: null, Reference: null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenPaymentExactlySettlesTheSupplierBalance_Succeeds()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns(100m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPaymentCommandHandler(repository, unitOfWork);
        var command = new RecordPaymentCommand(companyId, supplierId, null, 100m, PaymentDate: null, Reference: null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(-100m);
    }
}
