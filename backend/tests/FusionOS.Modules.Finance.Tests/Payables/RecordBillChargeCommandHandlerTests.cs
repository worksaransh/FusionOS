using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Commands.RecordBillCharge;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Domain.Payables;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Payables;

public class RecordBillChargeCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_PersistsAPositiveLedgerEntry()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, factRepository, unitOfWork);
        var command = new RecordBillChargeCommand(companyId, supplierId, purchaseOrderId, 500m, "PO 123 — office supplies");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(500m);
        result.SupplierId.Should().Be(supplierId);
        result.PurchaseOrderId.Should().Be(purchaseOrderId);
        await repository.Received(1).AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoPurchaseOrder_StillSucceeds()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, factRepository, unitOfWork);
        var command = new RecordBillChargeCommand(companyId, supplierId, null, 250m, "Ad-hoc consulting bill");

        var result = await handler.Handle(command, CancellationToken.None);

        result.PurchaseOrderId.Should().BeNull();
        result.Amount.Should().Be(250m);
    }

    [Fact]
    public async Task Handle_WhenNoFactExistsForThePurchaseOrder_AcceptsUnvalidated()
    {
        // Finance has learned nothing about this PO yet (no PurchaseOrderApproved
        // or PurchaseOrderGoodsReceiptCosted event consumed) — pre-three-way-match
        // behavior: the charge is accepted exactly as before this feature existed.
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        factRepository.GetByPurchaseOrderIdAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns((PurchaseOrderFact?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, factRepository, unitOfWork);
        var command = new RecordBillChargeCommand(companyId, supplierId, purchaseOrderId, 1_000_000m, "No fact known for this PO yet");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(1_000_000m);
    }

    [Fact]
    public async Task Handle_WhenChargeIsExactlyAtTheOrderedAmount_PersistsAPositiveLedgerEntry()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var fact = PurchaseOrderFact.FromApproval(companyId, purchaseOrderId, supplierId, 500m);
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountByPurchaseOrderAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(0m);
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        factRepository.GetByPurchaseOrderIdAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(fact);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, factRepository, unitOfWork);
        var command = new RecordBillChargeCommand(companyId, supplierId, purchaseOrderId, 500m, "Exactly at the ordered amount");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(500m);
        await repository.Received(1).AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenChargeWouldExceedTheOrderedAmount_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var fact = PurchaseOrderFact.FromApproval(companyId, purchaseOrderId, supplierId, 500m);
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountByPurchaseOrderAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(400m);
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        factRepository.GetByPurchaseOrderIdAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(fact);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, factRepository, unitOfWork);
        // 400 already charged + 200 requested = 600 > 500 ordered.
        var command = new RecordBillChargeCommand(companyId, supplierId, purchaseOrderId, 200m, "Over the ordered amount");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderedAmountIsNotYetKnown_SkipsThatLegAndValidatesOnlyReceivedAmount()
    {
        // The GRN-costed event beat the PurchaseOrderApproved event here (no
        // cross-topic ordering guarantee) — OrderedAmount is still null, so
        // that leg cannot be enforced yet; only the received-amount ceiling applies.
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var fact = PurchaseOrderFact.FromGoodsReceipt(companyId, purchaseOrderId, supplierId, 300m);
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountByPurchaseOrderAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(0m);
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        factRepository.GetByPurchaseOrderIdAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(fact);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, factRepository, unitOfWork);
        var command = new RecordBillChargeCommand(companyId, supplierId, purchaseOrderId, 1000m, "No ordered-amount fact yet, exceeds received");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenReceivedAmountIsStillZero_SkipsThatLegAndAllowsTheCharge()
    {
        // ReceivedAmount == 0 is indistinguishable from "no receipt facts have
        // arrived yet" — the received-amount ceiling is only enforced once at
        // least one costed receipt fact has arrived (PurchaseOrderFact policy).
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var fact = PurchaseOrderFact.FromApproval(companyId, purchaseOrderId, supplierId, 500m);
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountByPurchaseOrderAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(0m);
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        factRepository.GetByPurchaseOrderIdAsync(companyId, purchaseOrderId, Arg.Any<CancellationToken>()).Returns(fact);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, factRepository, unitOfWork);
        var command = new RecordBillChargeCommand(companyId, supplierId, purchaseOrderId, 500m, "No receipts consumed yet");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(500m);
    }
}
