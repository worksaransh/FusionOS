using FluentAssertions;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors.Events;
using Xunit;

namespace FusionOS.Modules.IntegrationHub.Tests.IntegrationConnectors;

public class IntegrationConnectorTests
{
    private static readonly Guid Company = Guid.NewGuid();

    [Fact]
    public void Create_WithValidFields_RaisesCreatedEvent()
    {
        var connector = IntegrationConnector.Create(Company, "shopify", "Shopify Store Sync", "Shopify", ConnectorCategory.Ecommerce);

        connector.Code.Should().Be("SHOPIFY");
        connector.Category.Should().Be(ConnectorCategory.Ecommerce);
        connector.IsActive.Should().BeTrue();
        connector.DomainEvents.Should().ContainSingle(e => e is IntegrationConnectorCreated);
    }

    [Fact]
    public void Create_WithBlankProvider_Throws()
    {
        var act = () => IntegrationConnector.Create(Company, "SHOPIFY", "Shopify Store Sync", "  ", ConnectorCategory.Ecommerce);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var connector = IntegrationConnector.Create(Company, "SHOPIFY", "Shopify Store Sync", "Shopify", ConnectorCategory.Ecommerce);

        connector.Deactivate();

        connector.IsActive.Should().BeFalse();
    }
}
