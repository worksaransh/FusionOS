using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Settings.Commands.UpdateCompanySettings;
using FusionOS.Modules.Core.Application.Settings.Contracts;
using FusionOS.Modules.Core.Domain.Settings;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Settings;

public class UpdateCompanySettingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSettingsAlreadyExist_UpdatesAndPersists()
    {
        var companyId = Guid.NewGuid();
        var existing = CompanySettings.CreateDefault(companyId);
        var repository = Substitute.For<ICompanySettingsRepository>();
        repository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(existing);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateCompanySettingsCommandHandler(repository, unitOfWork);
        var command = new UpdateCompanySettingsCommand(companyId, "INR", 100, "Acme Trading", "https://example.com/logo.png");

        var result = await handler.Handle(command, CancellationToken.None);

        result.DefaultCurrency.Should().Be("INR");
        result.DefaultPageSize.Should().Be(100);
        result.DisplayName.Should().Be("Acme Trading");
        await repository.DidNotReceive().AddAsync(Arg.Any<CompanySettings>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSettingsDoNotExistYet_CreatesThenUpdatesInOneStep()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ICompanySettingsRepository>();
        repository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns((CompanySettings?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateCompanySettingsCommandHandler(repository, unitOfWork);
        var command = new UpdateCompanySettingsCommand(companyId, "EUR", 10, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.DefaultCurrency.Should().Be("EUR");
        result.DefaultPageSize.Should().Be(10);
        await repository.Received(1).AddAsync(Arg.Any<CompanySettings>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
