using System.Net;
using FluentAssertions;
using Xunit;

namespace FusionOS.IntegrationTests;

/// <summary>/health must stay reachable without a token — it is what a load balancer/orchestrator polls.</summary>
public sealed class HealthEndpointTests : IClassFixture<FusionOSWebApplicationFactory>
{
    private readonly FusionOSWebApplicationFactory _factory;

    public HealthEndpointTests(FusionOSWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Health_ReturnsOk_WithoutAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
