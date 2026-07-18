using FusionOS.Modules.Procurement.Application.Rfqs.Commands.SendRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Rfqs;

public class SendRfqCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRfqIsDraft_SendsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(companyId, new[] { new RfqLineInput(Guid.NewGuid(), 10m) });
        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfq.Id, Arg.Any<CancellationToken>()).Returns(rfq);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new SendRfqCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new SendRfqCommand(companyId, rfq.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(RfqStatus.Sent));
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
        var handler = new SendRfqCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new SendRfqCommand(companyId, rfqId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
