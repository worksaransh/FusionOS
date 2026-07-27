using System.Net.Http.Headers;
using System.Text.Json;
using FusionOS.Modules.IntegrationHub.Application.Sync;
using Microsoft.Extensions.Configuration;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Sync;

/// <summary>
/// Phase 8 — AI &amp; Integrations. Syncs recent charges from Stripe's real
/// public API (https://stripe.com/docs/api/charges/list).
///
/// Auth: Bearer token — <c>Authorization: Bearer {secret_key}</c>, base URL
/// <c>https://api.stripe.com/v1/</c>. Config key: <c>{credentialReference}_SECRET_KEY</c>.
/// The secret key is read from <see cref="IConfiguration"/> at call time only
/// and never logged or persisted.
/// </summary>
public sealed class StripeConnectorSyncAdapter : IConnectorSyncAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public StripeConnectorSyncAdapter(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri("https://api.stripe.com/v1/");
        _configuration = configuration;
    }

    public string ProviderName => "Stripe";

    public async Task<ConnectorSyncResult> SyncAsync(Guid companyId, string? credentialReference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(credentialReference))
            return new ConnectorSyncResult(false, 0, "Stripe credentials not configured — set '{credentialReference}_SECRET_KEY'.");

        var secretKey = _configuration[$"{credentialReference}_SECRET_KEY"];
        if (string.IsNullOrWhiteSpace(secretKey))
            return new ConnectorSyncResult(false, 0, $"Stripe credentials not configured — set '{credentialReference}_SECRET_KEY'.");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "charges?limit=25");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ConnectorSyncResult(false, 0,
                    $"Stripe API request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                return new ConnectorSyncResult(true, 0, null);

            var processed = 0;
            foreach (var charge in data.EnumerateArray())
            {
                // Each item: id, amount (smallest currency unit), currency, status, receipt_email, created (unix seconds).
                var id = charge.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var amount = charge.TryGetProperty("amount", out var amountProp) ? amountProp.GetInt64() : 0;
                var currency = charge.TryGetProperty("currency", out var currencyProp) ? currencyProp.GetString() : null;
                var status = charge.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
                var receiptEmail = charge.TryGetProperty("receipt_email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String
                    ? emailProp.GetString()
                    : null;
                var createdUnix = charge.TryGetProperty("created", out var createdProp) ? createdProp.GetInt64() : 0;
                var createdAt = DateTimeOffset.FromUnixTimeSeconds(createdUnix);

                // See RazorpayConnectorSyncAdapter's equivalent comment — no
                // domain-event wiring yet, this pass proves fetch+parse only.
                _ = (id, amount, currency, status, receiptEmail, createdAt);
                processed++;
            }

            return new ConnectorSyncResult(true, processed, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return new ConnectorSyncResult(false, 0, $"Stripe sync failed: {ex.Message}");
        }
    }
}
