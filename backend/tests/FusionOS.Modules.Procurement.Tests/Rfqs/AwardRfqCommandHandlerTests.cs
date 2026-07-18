using FusionOS.Modules.Procurement.Application.Rfqs.Commands.AwardRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Rfqs;

public class AwardRfqCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithSubmittedQuote_AwardsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(companyId, new[] { new RfqLineInput(productId, 10m) });
        rfq.Send();
        var quote = rfq.SubmitSupplierQuote(Guid.NewGuid(), new[] { new SupplierQuoteLineInput(productId, 10m, 5m) });
        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfq.Id, Arg.Any<CancellationToken>()).Returns(rfq);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AwardRfqCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new AwardRfqCommand(companyId, rfq.Id, quote.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(RfqStatus.Awarded));
        result.AwardedSupplierQuoteId.Should().Be(quote.Id);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRfqDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var rfqId = Guid.NewGuid();
        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfqId, Arg.Any<CancellationToken>()).Returns((RequestForQuotation?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AwardRfqCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new AwardRfqCommand(companyId, rfqId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
