using FluentAssertions;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConnectConnector;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.DisconnectConnector;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.MarkConnectorConnectionError;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.ConnectorConnections;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.IntegrationHub.Tests.ConnectorConnections;

public class ConnectorConnectionCommandHandlerTests
{
    [Fact]
    public async Task ConnectConnector_WhenConnectorExists_PersistsConnectedConnection()
    {
        var companyId = Guid.NewGuid();
        var connectorId = Guid.NewGuid();
        var repository = Substitute.For<IConnectorConnectionRepository>();
        var connectorRepository = Substitute.For<IIntegrationConnectorRepository>();
        connectorRepository.ExistsAsync(companyId, connectorId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConnectConnectorCommandHandler(repository, connectorRepository, unitOfWork);

        var result = await handler.Handle(new ConnectConnectorCommand(companyId, connectorId, "Main Shopify Store"), CancellationToken.None);

        result.Status.Should().Be("Connected");
        await repository.Received(1).AddAsync(Arg.Any<ConnectorConnection>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConnectConnector_WhenConnectorMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IConnectorConnectionRepository>();
        var connectorRepository = Substitute.For<IIntegrationConnectorRepository>();
        connectorRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new ConnectConnectorCommandHandler(repository, connectorRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new ConnectConnectorCommand(companyId, Guid.NewGuid(), "Main Shopify Store"), CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task DisconnectConnector_ResolvesToDisconnected()
    {
        var companyId = Guid.NewGuid();
        var connection = Domain.ConnectorConnections.ConnectorConnection.Create(companyId, Guid.NewGuid(), "Main Shopify Store");
        var repository = Substitute.For<IConnectorConnectionRepository>();
        repository.GetByIdAsync(companyId, connection.Id, Arg.Any<CancellationToken>()).Returns(connection);
        var handler = new DisconnectConnectorCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new DisconnectConnectorCommand(companyId, connection.Id), CancellationToken.None);

        result.Status.Should().Be("Disconnected");
    }

    [Fact]
    public async Task MarkConnectorConnectionError_ResolvesToError()
    {
        var companyId = Guid.NewGuid();
        var connection = Domain.ConnectorConnections.ConnectorConnection.Create(companyId, Guid.NewGuid(), "Main Shopify Store");
        var repository = Substitute.For<IConnectorConnectionRepository>();
        repository.GetByIdAsync(companyId, connection.Id, Arg.Any<CancellationToken>()).Returns(connection);
        var handler = new MarkConnectorConnectionErrorCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new MarkConnectorConnectionErrorCommand(companyId, connection.Id), CancellationToken.None);

        result.Status.Should().Be("Error");
    }
}
