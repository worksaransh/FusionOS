using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.UpdateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.ExchangeRates;

public class UpdateExchangeRateCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenExchangeRateExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var exchangeRate = ExchangeRate.Create(companyId, "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.GetByIdAsync(companyId, exchangeRate.Id, Arg.Any<CancellationToken>()).Returns(exchangeRate);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateExchangeRateCommandHandler(repository, unitOfWork);
        var newDate = DateTimeOffset.UtcNow.AddDays(1);
        var command = new UpdateExchangeRateCommand(companyId, exchangeRate.Id, 0.95m, newDate);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Rate.Should().Be(0.95m);
        result.EffectiveDate.Should().Be(newDate);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExchangeRateDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var exchangeRateId = Guid.NewGuid();
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.GetByIdAsync(companyId, exchangeRateId, Arg.Any<CancellationToken>()).Returns((ExchangeRate?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateExchangeRateCommandHandler(repository, unitOfWork);
        var command = new UpdateExchangeRateCommand(companyId, exchangeRateId, 0.95m, DateTimeOffset.UtcNow);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNewEffectiveDateCollidesWithAnotherRate_Throws()
    {
        var companyId = Guid.NewGuid();
        var exchangeRate = ExchangeRate.Create(companyId, "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.GetByIdAsync(companyId, exchangeRate.Id, Arg.Any<CancellationToken>()).Returns(exchangeRate);
        var newDate = DateTimeOffset.UtcNow.AddDays(2);
        repository.RateExistsAsync(companyId, "USD", "EUR", newDate, exchangeRate.Id, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateExchangeRateCommandHandler(repository, unitOfWork);
        var command = new UpdateExchangeRateCommand(companyId, exchangeRate.Id, 0.95m, newDate);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }
}
