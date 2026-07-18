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
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, unitOfWork);
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
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordBillChargeCommandHandler(repository, unitOfWork);
        var command = new RecordBillChargeCommand(companyId, supplierId, null, 250m, "Ad-hoc consulting bill");

        var result = await handler.Handle(command, CancellationToken.None);

        result.PurchaseOrderId.Should().BeNull();
        result.Amount.Should().Be(250m);
    }
}
