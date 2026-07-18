using FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Rfqs;

public class CreateRfqCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidLines_PersistsRfq()
    {
        var repository = Substitute.For<IRfqRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateRfqCommandHandler(repository, unitOfWork);
        var command = new CreateRfqCommand(Guid.NewGuid(), new[] { new RfqLineInput(Guid.NewGuid(), 10m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(RfqStatus.Draft));
        result.Lines.Should().ContainSingle();
        await repository.Received(1).AddAsync(Arg.Any<RequestForQuotation>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
