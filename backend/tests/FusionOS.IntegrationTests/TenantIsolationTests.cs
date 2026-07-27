using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace FusionOS.IntegrationTests;

/// <summary>
/// Proves Company A cannot read Company B's data through a real HTTP call —
/// the multi-tenant boundary 04_DATABASE_GUIDELINES.md §6 depends on and that
/// unit tests (which construct one company at a time) can't exercise end to
/// end. Two independent companies, each with their own bootstrap Owner, are
/// seeded via FusionOSWebApplicationFactory.SeedCompanyWithOwnerAsync (see its
/// doc comment for why seeding — not the public /register endpoint — is used
/// to create them); every assertion below is a genuine authenticated HTTP call
/// against the real Host.
///
/// Two distinct cross-tenant scenarios are covered, because this codebase
/// enforces them at two different points with two different outcomes:
///  1. Passing another tenant's CompanyId as the query parameter is rejected
///     with 403 by TenantIsolationBehavior (BuildingBlocks.Application) before
///     any repository query runs — it never even gets a chance to leak data.
///  2. Passing your OWN CompanyId (so TenantIsolationBehavior's check passes)
///     but another tenant's record id is a 404, not a 403 or a leaked record —
///     ILeadRepository.GetByIdAsync filters by (companyId, leadId) together,
///     so a real record that simply doesn't belong to your company is
///     indistinguishable from one that doesn't exist at all.
/// </summary>
public sealed class TenantIsolationTests : IClassFixture<FusionOSWebApplicationFactory>
{
    private readonly FusionOSWebApplicationFactory _factory;

    public TenantIsolationTests(FusionOSWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_LeadFromOtherCompany_UsingOwnCompanyId_ReturnsNotFound()
    {
        var (companyAId, _, ownerAEmail, ownerAPassword) = await _factory.SeedCompanyWithOwnerAsync("Tenant Isolation Co A");
        var (companyBId, _, ownerBEmail, ownerBPassword) = await _factory.SeedCompanyWithOwnerAsync("Tenant Isolation Co B");

        var clientA = _factory.CreateClient();
        var tokenA = await LoginAsync(clientA, ownerAEmail, ownerAPassword, companyAId);
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createResponse = await clientA.PostAsJsonAsync("/api/v1/crm/leads", new
        {
            companyId = companyAId,
            name = "Company A's Confidential Lead",
            contactEmail = (string?)null,
            contactPhone = (string?)null,
            source = "web",
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdLead = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var leadId = createdLead.GetProperty("id").GetGuid();

        var clientB = _factory.CreateClient();
        var tokenB = await LoginAsync(clientB, ownerBEmail, ownerBPassword, companyBId);
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        // Company B's owner, scoped to their OWN company (satisfies
        // TenantIsolationBehavior), asking for a lead id that only exists under
        // Company A — must not resolve, and must not distinguish "exists in
        // another tenant" from "never existed" via a different status code.
        var response = await clientB.GetAsync($"/api/v1/crm/leads/{leadId}?companyId={companyBId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_Lead_UsingAnotherCompanysIdAsTheQueryParameter_ReturnsForbidden()
    {
        var (companyAId, _, ownerAEmail, ownerAPassword) = await _factory.SeedCompanyWithOwnerAsync("Tenant Isolation Co C");
        var (companyBId, _, _, _) = await _factory.SeedCompanyWithOwnerAsync("Tenant Isolation Co D");

        var clientA = _factory.CreateClient();
        var tokenA = await LoginAsync(clientA, ownerAEmail, ownerAPassword, companyAId);
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createResponse = await clientA.PostAsJsonAsync("/api/v1/crm/leads", new
        {
            companyId = companyAId,
            name = "Company A's Other Lead",
            contactEmail = (string?)null,
            contactPhone = (string?)null,
            source = "web",
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdLead = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var leadId = createdLead.GetProperty("id").GetGuid();

        // Still Company A's own token, but now claiming Company B's id as the
        // query parameter — this is the "just change the query string" attack,
        // and TenantIsolationBehavior rejects it before the repository is ever
        // queried, regardless of which company actually owns the lead id.
        var response = await clientA.GetAsync($"/api/v1/crm/leads/{leadId}?companyId={companyBId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task List_Leads_ForCompanyB_DoesNotIncludeCompanyAsLeads()
    {
        var (companyAId, _, ownerAEmail, ownerAPassword) = await _factory.SeedCompanyWithOwnerAsync("Tenant Isolation Co E");
        var (companyBId, _, ownerBEmail, ownerBPassword) = await _factory.SeedCompanyWithOwnerAsync("Tenant Isolation Co F");

        var clientA = _factory.CreateClient();
        var tokenA = await LoginAsync(clientA, ownerAEmail, ownerAPassword, companyAId);
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        (await clientA.PostAsJsonAsync("/api/v1/crm/leads", new
        {
            companyId = companyAId,
            name = "Company A Lead One",
            contactEmail = (string?)null,
            contactPhone = (string?)null,
            source = "web",
        })).StatusCode.Should().Be(HttpStatusCode.Created);

        var clientB = _factory.CreateClient();
        var tokenB = await LoginAsync(clientB, ownerBEmail, ownerBPassword, companyBId);
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var response = await clientB.GetAsync($"/api/v1/crm/leads?companyId={companyBId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        page.GetProperty("data").GetArrayLength().Should().Be(0);
        page.GetProperty("totalCount").GetInt32().Should().Be(0);
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
