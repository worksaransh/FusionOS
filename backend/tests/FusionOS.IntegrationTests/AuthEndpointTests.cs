using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace FusionOS.IntegrationTests;

/// <summary>
/// Covers the two most safety-critical behaviors added this phase
/// (07_SECURITY.md): every endpoint requires auth by default, and login
/// failures come back as a proper RFC 7807 problem response, never a 500.
/// </summary>
public sealed class AuthEndpointTests : IClassFixture<FusionOSWebApplicationFactory>
{
    private readonly FusionOSWebApplicationFactory _factory;

    public AuthEndpointTests(FusionOSWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/core/companies");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Login_WithUnknownEmail_ReturnsBadRequest_NotServerError()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/core/auth/login", new
        {
            email = "no-such-user@example.com",
            password = "whatever-password",
            companyId = (Guid?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("status").GetInt32().Should().Be(400);
    }
}
