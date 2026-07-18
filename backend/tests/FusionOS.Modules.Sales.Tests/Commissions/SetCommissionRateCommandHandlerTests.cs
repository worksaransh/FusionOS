using FusionOS.Modules.Sales.Application.Commissions.Commands.SetCommissionRate;
using FusionOS.Modules.Sales.Application.Commissions.Contracts;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Domain.Commissions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Commissions;

public class SetCommissionRateCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithNoExistingRate_CreatesNewRate()
    {
        var repository = Substitute.For<ISalesCommissionRateRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        repository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((SalesCommissionRate?)null);
        var handler = new SetCommissionRateCommandHandler(repository, unitOfWork);
        var command = new SetCommissionRateCommand(Guid.NewGuid(), Guid.NewGuid(), 5m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.RatePercentage.Should().Be(5m);
        await repository.Received(1).AddAsync(Arg.Any<SalesCommissionRate>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingRate_OverwritesRate()
    {
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = SalesCommissionRate.Create(companyId, userId, 5m);
        var repository = Substitute.For<ISalesCommissionRateRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        repository.GetByUserIdAsync(companyId, userId, Arg.Any<CancellationToken>()).Returns(existing);
        var handler = new SetCommissionRateCommandHandler(repository, unitOfWork);
        var command = new SetCommissionRateCommand(companyId, userId, 12m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.RatePercentage.Should().Be(12m);
        await repository.DidNotReceive().AddAsync(Arg.Any<SalesCommissionRate>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
