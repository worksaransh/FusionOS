using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.ExchangeRates;

public class CreateExchangeRateCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNoRateExistsForTheDate_PersistsExchangeRate()
    {
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.RateExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateExchangeRateCommandHandler(repository, unitOfWork);
        var command = new CreateExchangeRateCommand(Guid.NewGuid(), "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);

        var result = await handler.Handle(command, CancellationToken.None);

        result.FromCurrencyCode.Should().Be("USD");
        result.ToCurrencyCode.Should().Be("EUR");
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.ExchangeRates.ExchangeRate>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRateAlreadyExistsForTheDate_Throws()
    {
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.RateExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateExchangeRateCommandHandler(repository, unitOfWork);
        var command = new CreateExchangeRateCommand(Guid.NewGuid(), "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
