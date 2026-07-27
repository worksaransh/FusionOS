using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace FusionOS.IntegrationTests;

/// <summary>
/// One full CRUD-ish lifecycle exercised over real HTTP end to end: create →
/// get-by-id → list → qualify → disqualify. CRM Leads (FusionOS.Modules.Crm)
/// was picked over Manufacturing's BillOfMaterials per the task's own
/// "simplest dependencies" guidance — a Lead only needs a CompanyId plus a
/// name (no cross-module foreign keys to seed first, unlike a BillOfMaterials'
/// required ProductId from Inventory).
///
/// Leads have no PUT "update" endpoint (see LeadsController — there is no
/// UpdateLeadCommand), so this test uses the closest real equivalents:
/// Qualify (New → Qualified, the one legal free-form state transition off the
/// initial status) standing in for "update", and Disqualify (→ Disqualified,
/// a soft terminal state — Leads are never hard-deleted) standing in for
/// "deactivate".
/// </summary>
public sealed class LeadLifecycleTests : IClassFixture<FusionOSWebApplicationFactory>
{
    private readonly FusionOSWebApplicationFactory _factory;

    public LeadLifecycleTests(FusionOSWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task FullLifecycle_Create_GetById_List_Qualify_Disqualify()
    {
        var (companyId, _, ownerEmail, ownerPassword) = await _factory.SeedCompanyWithOwnerAsync("Lead Lifecycle Co");

        var client = _factory.CreateClient();
        var accessToken = await LoginAsync(client, ownerEmail, ownerPassword, companyId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // --- Create ---------------------------------------------------------
        var createResponse = await client.PostAsJsonAsync("/api/v1/crm/leads", new
        {
            companyId,
            name = "Acme Rooftop Solar",
            contactEmail = "buyer@acme.example",
            contactPhone = "+1-555-0100",
            source = "web",
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        created.GetProperty("name").GetString().Should().Be("Acme Rooftop Solar");
        created.GetProperty("status").GetString().Should().Be("New");
        var leadId = created.GetProperty("id").GetGuid();

        // --- Get by id --------------------------------------------------------
        var getResponse = await client.GetAsync($"/api/v1/crm/leads/{leadId}?companyId={companyId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        fetched.GetProperty("id").GetGuid().Should().Be(leadId);
        fetched.GetProperty("contactEmail").GetString().Should().Be("buyer@acme.example");
        fetched.GetProperty("status").GetString().Should().Be("New");

        // --- List ---------------------------------------------------------
        var listResponse = await client.GetAsync($"/api/v1/crm/leads?companyId={companyId}");

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        page.GetProperty("totalCount").GetInt32().Should().Be(1);
        var listedLead = page.GetProperty("data").EnumerateArray().Single();
        listedLead.GetProperty("id").GetGuid().Should().Be(leadId);

        // --- "Update" (Qualify: New -> Qualified) --------------------------
        var qualifyResponse = await client.PostAsJsonAsync($"/api/v1/crm/leads/{leadId}/qualify", new { companyId });

        qualifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var qualified = await qualifyResponse.Content.ReadFromJsonAsync<JsonElement>();
        qualified.GetProperty("status").GetString().Should().Be("Qualified");

        // Qualifying twice is not a valid transition (New -> Qualified only) —
        // confirms the state machine is enforced, not just accepted blindly.
        var secondQualifyResponse = await client.PostAsJsonAsync($"/api/v1/crm/leads/{leadId}/qualify", new { companyId });
        secondQualifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // --- "Deactivate" (Disqualify) --------------------------------------
        var disqualifyResponse = await client.PostAsJsonAsync($"/api/v1/crm/leads/{leadId}/disqualify", new { companyId });

        disqualifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var disqualified = await disqualifyResponse.Content.ReadFromJsonAsync<JsonElement>();
        disqualified.GetProperty("status").GetString().Should().Be("Disqualified");

        // Confirm the disqualification is really persisted, not just echoed back.
        var finalGetResponse = await client.GetAsync($"/api/v1/crm/leads/{leadId}?companyId={companyId}");
        finalGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var final = await finalGetResponse.Content.ReadFromJsonAsync<JsonElement>();
        final.GetProperty("status").GetString().Should().Be("Disqualified");

        // A disqualified lead cannot be disqualified again — it's a terminal state.
        var secondDisqualifyResponse = await client.PostAsJsonAsync($"/api/v1/crm/leads/{leadId}/disqualify", new { companyId });
        secondDisqualifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
