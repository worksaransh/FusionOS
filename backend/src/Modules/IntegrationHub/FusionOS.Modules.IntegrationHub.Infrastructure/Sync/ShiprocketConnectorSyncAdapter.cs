using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FusionOS.Modules.IntegrationHub.Application.Sync;
using Microsoft.Extensions.Configuration;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Sync;

/// <summary>
/// Phase 8 — AI &amp; Integrations. Syncs recent orders from Shiprocket's real
/// public API (https://apidocs.shiprocket.in/).
///
/// Auth: Shiprocket has no static API key — every session starts with a login
/// call, <c>POST https://apiv2.shiprocket.in/v1/external/auth/login</c> with a
/// JSON body <c>{ "email": ..., "password": ... }</c>, which returns a bearer
/// <c>token</c> used as <c>Authorization: Bearer {token}</c> on subsequent
/// calls. Config keys: <c>{credentialReference}_EMAIL</c> and
/// <c>{credentialReference}_PASSWORD</c>.
///
/// This pass performs the login call fresh on every <see cref="SyncAsync"/>
/// run rather than caching the token — Shiprocket tokens are valid for a
/// period (historically ~10 days) so caching the token (e.g. alongside the
/// connection, with an expiry) is a reasonable future optimization to avoid
/// one extra HTTP round trip per sync, but is out of scope here.
/// </summary>
public sealed class ShiprocketConnectorSyncAdapter : IConnectorSyncAdapter
{
    private const string AuthBaseUrl = "https://apiv2.shiprocket.in/v1/external/";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ShiprocketConnectorSyncAdapter(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri(AuthBaseUrl);
        _configuration = configuration;
    }

    public string ProviderName => "Shiprocket";

    public async Task<ConnectorSyncResult> SyncAsync(Guid companyId, string? credentialReference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(credentialReference))
            return new ConnectorSyncResult(false, 0, "Shiprocket authentication failed — check the configured email/password.");

        var email = _configuration[$"{credentialReference}_EMAIL"];
        var password = _configuration[$"{credentialReference}_PASSWORD"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return new ConnectorSyncResult(false, 0, "Shiprocket authentication failed — check the configured email/password.");

        try
        {
            var token = await LoginAsync(email, password, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return new ConnectorSyncResult(false, 0, "Shiprocket authentication failed — check the configured email/password.");

            using var request = new HttpRequestMessage(HttpMethod.Get, "orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ConnectorSyncResult(false, 0,
                    $"Shiprocket orders request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                return new ConnectorSyncResult(true, 0, null);

            var processed = 0;
            foreach (var order in data.EnumerateArray())
            {
                // Fields vary by account/plan but typically include: id,
                // channel_order_id, customer_name, total, status.
                var id = order.TryGetProperty("id", out var idProp) ? idProp.ToString() : null;
                var channelOrderId = order.TryGetProperty("channel_order_id", out var channelProp) ? channelProp.GetString() : null;
                var customerName = order.TryGetProperty("customer_name", out var customerProp) ? customerProp.GetString() : null;
                var total = order.TryGetProperty("total", out var totalProp) ? totalProp.ToString() : null;
                var status = order.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;

                // See RazorpayConnectorSyncAdapter's equivalent comment — no
                // domain-event wiring yet, this pass proves fetch+parse only.
                _ = (id, channelOrderId, customerName, total, status);
                processed++;
            }

            return new ConnectorSyncResult(true, processed, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return new ConnectorSyncResult(false, 0, $"Shiprocket sync failed: {ex.Message}");
        }
    }

    private async Task<string?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "auth/login")
        {
            Content = JsonContent.Create(new { email, password }),
        };

        using var loginResponse = await _httpClient.SendAsync(loginRequest, cancellationToken);
        if (!loginResponse.IsSuccessStatusCode)
            return null;

        await using var stream = await loginResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return document.RootElement.TryGetProperty("token", out var tokenProp) ? tokenProp.GetString() : null;
    }
}
