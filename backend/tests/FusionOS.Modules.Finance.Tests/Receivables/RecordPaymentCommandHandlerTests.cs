using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Commands.RecordPayment;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Domain.Receivables;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Receivables;

/// <summary>
/// Covers RecordPaymentCommand (Phase M4, 2026-07-15) — including the
/// overpayment guard: a payment can never exceed the invoice's current
/// outstanding balance (SumAmountByInvoiceAsync), mirroring the "don't let a
/// transaction exceed what's actually owed" rule already enforced by Sales'
/// CreateInvoice/CreateDispatch against a Sales Order's ordered quantity.
/// </summary>
public class RecordPaymentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPaymentIsWithinOutstandingBalance_PersistsANegativeLedgerEntry()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var repository = Substitute.For<IArLedgerRepository>();
        repository.SumAmountByInvoiceAsync(companyId, invoiceId, Arg.Any<CancellationToken>()).Returns(250m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPaymentCommandHandler(repository, unitOfWork);
        var command = new RecordPaymentCommand(companyId, customerId, invoiceId, 100m, PaymentDate: null, Reference: "UTR1");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(-100m);
        result.InvoiceId.Should().Be(invoiceId);
        await repository.Received(1).AddAsync(Arg.Any<ArLedgerEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPaymentExceedsOutstandingBalance_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var repository = Substitute.For<IArLedgerRepository>();
        repository.SumAmountByInvoiceAsync(companyId, invoiceId, Arg.Any<CancellationToken>()).Returns(50m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPaymentCommandHandler(repository, unitOfWork);
        var command = new RecordPaymentCommand(companyId, customerId, invoiceId, 100m, PaymentDate: null, Reference: null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<ArLedgerEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInvoiceHasNoChargesYet_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var repository = Substitute.For<IArLedgerRepository>();
        repository.SumAmountByInvoiceAsync(companyId, invoiceId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPaymentCommandHandler(repository, unitOfWork);
        var command = new RecordPaymentCommand(companyId, customerId, invoiceId, 1m, PaymentDate: null, Reference: null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenPaymentExactlySettlesTheInvoice_Succeeds()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var repository = Substitute.For<IArLedgerRepository>();
        repository.SumAmountByInvoiceAsync(companyId, invoiceId, Arg.Any<CancellationToken>()).Returns(100m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPaymentCommandHandler(repository, unitOfWork);
        var command = new RecordPaymentCommand(companyId, customerId, invoiceId, 100m, PaymentDate: null, Reference: null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(-100m);
    }
}
