using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace FusionOS.IntegrationTests;

/// <summary>
/// RBAC enforcement (07_SECURITY.md §2) exercised over real HTTP, one level
/// deeper than AuthEndpointTests' "every endpoint requires SOME token" check:
/// a real, authenticated user whose role grants none (or the wrong ones) of
/// the permissions a command/query requires must still be rejected with 403,
/// not merely let through because they hold a valid token.
///
/// <b>What is and isn't achievable with the current factory:</b> the Company +
/// Owner user in every test below is seeded directly through
/// FusionOSWebApplicationFactory.SeedCompanyWithOwnerAsync /
/// SeedUserWithPermissionsAsync (repository calls, not HTTP), because the
/// public POST /api/v1/core/auth/register bootstrap path is currently
/// unreachable for an anonymous caller — see SeedCompanyWithOwnerAsync's doc
/// comment for the TenantIsolationBehavior gap that causes this. Everything
/// AFTER that seeding step (login, every assertion below) goes over genuine
/// HTTP against the real Host, so these tests still verify the real
/// AuthorizationBehavior/TenantIsolationBehavior pipeline, not a mock.
/// </summary>
public sealed class RbacEnforcementTests : IClassFixture<FusionOSWebApplicationFactory>
{
    private readonly FusionOSWebApplicationFactory _factory;

    public RbacEnforcementTests(FusionOSWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Leads_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/crm/leads?companyId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Leads_AuthenticatedWithNoPermissionsAtAll_ReturnsForbidden()
    {
        var (companyId, _, _, _) = await _factory.SeedCompanyWithOwnerAsync("Rbac No-Permission Co");
        // Deliberately narrower than even the zero-permission "Member" role a real
        // teammate invite lands on — proves a token alone is not enough.
        var (_, memberEmail, memberPassword) = await _factory.SeedUserWithPermissionsAsync(companyId, Array.Empty<string>());

        var client = _factory.CreateClient();
        var accessToken = await LoginAsync(client, memberEmail, memberPassword, companyId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"/api/v1/crm/leads?companyId={companyId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_Leads_AuthenticatedWithReadOnlyPermission_CannotCreate()
    {
        var (companyId, _, _, _) = await _factory.SeedCompanyWithOwnerAsync("Rbac Read-Only Co");
        // Granted crm.lead.read but deliberately not crm.lead.create — proves the
        // check is per-action, not "holds some permission in this module".
        var (_, memberEmail, memberPassword) = await _factory.SeedUserWithPermissionsAsync(companyId, new[] { "crm.lead.read" });

        var client = _factory.CreateClient();
        var accessToken = await LoginAsync(client, memberEmail, memberPassword, companyId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await client.PostAsJsonAsync("/api/v1/crm/leads", new
        {
            companyId,
            name = "Should Never Be Created",
            contactEmail = (string?)null,
            contactPhone = (string?)null,
            source = "web",
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // The same user's granted crm.lead.read DOES work — confirms the 403 above
        // was specifically about the missing "create" permission, not a broken token.
        var listResponse = await client.GetAsync($"/api/v1/crm/leads?companyId={companyId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        page.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Get_Leads_AuthenticatedOwner_WithFullPermissions_Succeeds()
    {
        var (companyId, _, ownerEmail, ownerPassword) = await _factory.SeedCompanyWithOwnerAsync("Rbac Owner Co");

        var client = _factory.CreateClient();
        var accessToken = await LoginAsync(client, ownerEmail, ownerPassword, companyId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"/api/v1/crm/leads?companyId={companyId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password, Guid companyId)
    {
        var response = await client.PostAsJsonAsync("/api/v1/core/auth/login", new
        {
            email,
            password,
            companyId = (Guid?)companyId,
        });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("accessToken").GetString()!;
    }
}
