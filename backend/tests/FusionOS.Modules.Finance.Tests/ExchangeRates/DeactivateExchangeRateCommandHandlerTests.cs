using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.DeactivateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.ExchangeRates;

public class DeactivateExchangeRateCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenExchangeRateExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var exchangeRate = ExchangeRate.Create(companyId, "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.GetByIdAsync(companyId, exchangeRate.Id, Arg.Any<CancellationToken>()).Returns(exchangeRate);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateExchangeRateCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateExchangeRateCommand(companyId, exchangeRate.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
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
        var handler = new DeactivateExchangeRateCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateExchangeRateCommand(companyId, exchangeRateId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
