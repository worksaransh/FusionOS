using FluentAssertions;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.DisablePluginInstallation;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.EnablePluginInstallation;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.InstallPlugin;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.UninstallPlugin;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using FusionOS.Modules.Marketplace.Domain.PluginInstallations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Marketplace.Tests.PluginInstallations;

public class PluginInstallationCommandHandlerTests
{
    [Fact]
    public async Task InstallPlugin_WhenListingExists_PersistsInstalledInstallation()
    {
        var companyId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var repository = Substitute.For<IPluginInstallationRepository>();
        var listingRepository = Substitute.For<IPluginListingRepository>();
        listingRepository.ExistsAsync(companyId, listingId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new InstallPluginCommandHandler(repository, listingRepository, unitOfWork);

        var result = await handler.Handle(new InstallPluginCommand(companyId, listingId), CancellationToken.None);

        result.Status.Should().Be("Installed");
        await repository.Received(1).AddAsync(Arg.Any<PluginInstallation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InstallPlugin_WhenListingMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPluginInstallationRepository>();
        var listingRepository = Substitute.For<IPluginListingRepository>();
        listingRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new InstallPluginCommandHandler(repository, listingRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new InstallPluginCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task DisableThenEnable_RoundTrips()
    {
        var companyId = Guid.NewGuid();
        var installation = Domain.PluginInstallations.PluginInstallation.Create(companyId, Guid.NewGuid());
        var repository = Substitute.For<IPluginInstallationRepository>();
        repository.GetByIdAsync(companyId, installation.Id, Arg.Any<CancellationToken>()).Returns(installation);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var disableHandler = new DisablePluginInstallationCommandHandler(repository, unitOfWork);
        var disabled = await disableHandler.Handle(new DisablePluginInstallationCommand(companyId, installation.Id), CancellationToken.None);
        disabled.Status.Should().Be("Disabled");

        var enableHandler = new EnablePluginInstallationCommandHandler(repository, unitOfWork);
        var enabled = await enableHandler.Handle(new EnablePluginInstallationCommand(companyId, installation.Id), CancellationToken.None);
        enabled.Status.Should().Be("Installed");
    }

    [Fact]
    public async Task UninstallPlugin_ResolvesToUninstalled()
    {
        var companyId = Guid.NewGuid();
        var installation = Domain.PluginInstallations.PluginInstallation.Create(companyId, Guid.NewGuid());
        var repository = Substitute.For<IPluginInstallationRepository>();
        repository.GetByIdAsync(companyId, installation.Id, Arg.Any<CancellationToken>()).Returns(installation);
        var handler = new UninstallPluginCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new UninstallPluginCommand(companyId, installation.Id), CancellationToken.None);

        result.Status.Should().Be("Uninstalled");
    }
}
