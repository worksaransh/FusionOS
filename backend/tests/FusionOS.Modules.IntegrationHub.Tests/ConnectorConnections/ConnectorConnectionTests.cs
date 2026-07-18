using FluentAssertions;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections.Events;
using Xunit;

namespace FusionOS.Modules.IntegrationHub.Tests.ConnectorConnections;

public class ConnectorConnectionTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Connector = Guid.NewGuid();

    private static ConnectorConnection New() => ConnectorConnection.Create(Company, Connector, "Main Shopify Store");

    [Fact]
    public void Create_Connected_RaisesConnectedEvent()
    {
        var connection = New();

        connection.Status.Should().Be(ConnectionStatus.Connected);
        connection.DomainEvents.Should().ContainSingle(e => e is ConnectorConnected);
    }

    [Fact]
    public void Create_WithBlankLabel_Throws()
    {
        var act = () => ConnectorConnection.Create(Company, Connector, "  ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Disconnect_FromConnected_Transitions()
    {
        var connection = New();

        connection.Disconnect();

        connection.Status.Should().Be(ConnectionStatus.Disconnected);
        connection.DisconnectedAt.Should().NotBeNull();
    }

    [Fact]
    public void Disconnect_Twice_Throws()
    {
        var connection = New();
        connection.Disconnect();

        var act = () => connection.Disconnect();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkError_FromConnected_Transitions()
    {
        var connection = New();

        connection.MarkError();

        connection.Status.Should().Be(ConnectionStatus.Error);
    }

    [Fact]
    public void MarkError_WhenDisconnected_Throws()
    {
        var connection = New();
        connection.Disconnect();

        var act = () => connection.MarkError();

        act.Should().Throw<InvalidOperationException>();
    }
}
