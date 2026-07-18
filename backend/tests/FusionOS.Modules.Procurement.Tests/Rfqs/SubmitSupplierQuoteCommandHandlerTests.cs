using FusionOS.Modules.Procurement.Application.Rfqs.Commands.SubmitSupplierQuote;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Rfqs;

public class SubmitSupplierQuoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSupplierExists_AddsQuoteAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(companyId, new[] { new RfqLineInput(productId, 10m) });
        rfq.Send();
        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfq.Id, Arg.Any<CancellationToken>()).Returns(rfq);
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SubmitSupplierQuoteCommandHandler(repository, supplierRepository, unitOfWork);
        var supplierId = Guid.NewGuid();
        var command = new SubmitSupplierQuoteCommand(companyId, rfq.Id, supplierId, new[] { new SupplierQuoteLineInput(productId, 10m, 5m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.SupplierQuotes.Should().ContainSingle(q => q.SupplierId == supplierId && q.TotalAmount == 50m);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSupplierDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(companyId, new[] { new RfqLineInput(productId, 10m) });
        rfq.Send();
        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfq.Id, Arg.Any<CancellationToken>()).Returns(rfq);
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SubmitSupplierQuoteCommandHandler(repository, supplierRepository, unitOfWork);
        var command = new SubmitSupplierQuoteCommand(companyId, rfq.Id, Guid.NewGuid(), new[] { new SupplierQuoteLineInput(productId, 10m, 5m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenRfqDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var rfqId = Guid.NewGuid();
        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfqId, Arg.Any<CancellationToken>()).Returns((RequestForQuotation?)null);
        var supplierRepository = Substitute.For<ISupplierRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SubmitSupplierQuoteCommandHandler(repository, supplierRepository, unitOfWork);
        var command = new SubmitSupplierQuoteCommand(companyId, rfqId, Guid.NewGuid(), new[] { new SupplierQuoteLineInput(Guid.NewGuid(), 10m, 5m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
