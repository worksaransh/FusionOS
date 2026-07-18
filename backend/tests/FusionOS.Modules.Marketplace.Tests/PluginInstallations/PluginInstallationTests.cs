using FluentAssertions;
using FusionOS.Modules.Marketplace.Domain.PluginInstallations;
using FusionOS.Modules.Marketplace.Domain.PluginInstallations.Events;
using Xunit;

namespace FusionOS.Modules.Marketplace.Tests.PluginInstallations;

public class PluginInstallationTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Listing = Guid.NewGuid();

    private static PluginInstallation New() => PluginInstallation.Create(Company, Listing);

    [Fact]
    public void Create_Installed_RaisesInstalledEvent()
    {
        var installation = New();

        installation.Status.Should().Be(InstallationStatus.Installed);
        installation.DomainEvents.Should().ContainSingle(e => e is PluginInstalled);
    }

    [Fact]
    public void Create_WithEmptyPluginListingId_Throws()
    {
        var act = () => PluginInstallation.Create(Company, Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Disable_FromInstalled_Transitions()
    {
        var installation = New();

        installation.Disable();

        installation.Status.Should().Be(InstallationStatus.Disabled);
    }

    [Fact]
    public void Enable_FromDisabled_Transitions()
    {
        var installation = New();
        installation.Disable();

        installation.Enable();

        installation.Status.Should().Be(InstallationStatus.Installed);
    }

    [Fact]
    public void Enable_WhenNotDisabled_Throws()
    {
        var installation = New();

        var act = () => installation.Enable();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Uninstall_FromInstalled_SetsTerminal()
    {
        var installation = New();

        installation.Uninstall();

        installation.Status.Should().Be(InstallationStatus.Uninstalled);
        installation.UninstalledAt.Should().NotBeNull();
    }

    [Fact]
    public void Uninstall_Twice_Throws()
    {
        var installation = New();
        installation.Uninstall();

        var act = () => installation.Uninstall();

        act.Should().Throw<InvalidOperationException>();
    }
}
