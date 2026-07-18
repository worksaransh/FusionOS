using FluentAssertions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.CreateIntegrationConnector;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.DeactivateIntegrationConnector;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.IntegrationHub.Tests.IntegrationConnectors;

public class IntegrationConnectorCommandHandlerTests
{
    [Fact]
    public async Task CreateIntegrationConnector_PersistsActiveConnector()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IIntegrationConnectorRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateIntegrationConnectorCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateIntegrationConnectorCommand(companyId, "SHOPIFY", "Shopify Store Sync", "Shopify", ConnectorCategory.Ecommerce),
            CancellationToken.None);

        result.Code.Should().Be("SHOPIFY");
        result.IsActive.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<IntegrationConnector>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateIntegrationConnector_SetsInactive()
    {
        var companyId = Guid.NewGuid();
        var connector = IntegrationConnector.Create(companyId, "SHOPIFY", "Shopify Store Sync", "Shopify", ConnectorCategory.Ecommerce);
        var repository = Substitute.For<IIntegrationConnectorRepository>();
        repository.GetByIdAsync(companyId, connector.Id, Arg.Any<CancellationToken>()).Returns(connector);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateIntegrationConnectorCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateIntegrationConnectorCommand(companyId, connector.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateIntegrationConnector_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IIntegrationConnectorRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IntegrationConnector?)null);
        var handler = new DeactivateIntegrationConnectorCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateIntegrationConnectorCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
