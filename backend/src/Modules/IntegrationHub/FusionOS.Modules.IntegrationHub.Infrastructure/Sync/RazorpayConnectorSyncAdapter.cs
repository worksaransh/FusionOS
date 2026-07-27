using System.Net.Http.Headers;
using System.Text.Json;
using FusionOS.Modules.IntegrationHub.Application.Sync;
using Microsoft.Extensions.Configuration;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Sync;

/// <summary>
/// Phase 8 — AI &amp; Integrations. Syncs recently-received payments from
/// Razorpay's real public API (https://razorpay.com/docs/api/payments/).
///
/// Auth: Razorpay uses HTTP Basic Auth over the pair (key_id, key_secret) —
/// <c>Authorization: Basic base64(key_id:key_secret)</c>, base URL
/// <c>https://api.razorpay.com/v1/</c>. Razorpay needs two secret halves, so
/// unlike a single-secret provider, this adapter's <c>credentialReference</c>
/// names TWO configuration keys rather than one:
///   - <c>{credentialReference}_KEY_ID</c>
///   - <c>{credentialReference}_KEY_SECRET</c>
/// Neither the key id nor the key secret is ever logged, persisted, or
/// returned in a <see cref="ConnectorSyncResult"/> — both are read from
/// <see cref="IConfiguration"/> at call time only and held in a local
/// variable for the duration of the HTTP call.
/// </summary>
public sealed class RazorpayConnectorSyncAdapter : IConnectorSyncAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RazorpayConnectorSyncAdapter(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri("https://api.razorpay.com/v1/");
        _configuration = configuration;
    }

    public string ProviderName => "Razorpay";

    public async Task<ConnectorSyncResult> SyncAsync(Guid companyId, string? credentialReference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(credentialReference))
            return new ConnectorSyncResult(false, 0, "Razorpay credentials not fully configured — set both '{credentialReference}_KEY_ID' and '{credentialReference}_KEY_SECRET'.");

        var keyId = _configuration[$"{credentialReference}_KEY_ID"];
        var keySecret = _configuration[$"{credentialReference}_KEY_SECRET"];

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
        {
            return new ConnectorSyncResult(false, 0,
                $"Razorpay credentials not fully configured — set both '{credentialReference}_KEY_ID' and '{credentialReference}_KEY_SECRET'.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "payments");
            var basicAuthValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ConnectorSyncResult(false, 0,
                    $"Razorpay API request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
                return new ConnectorSyncResult(true, 0, null);

            var processed = 0;
            foreach (var payment in items.EnumerateArray())
            {
                // Each item: id, amount (paise), currency, status, email, created_at (unix seconds).
                var id = payment.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var amountPaise = payment.TryGetProperty("amount", out var amountProp) ? amountProp.GetInt64() : 0;
                var currency = payment.TryGetProperty("currency", out var currencyProp) ? currencyProp.GetString() : null;
                var status = payment.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
                var email = payment.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                var createdAtUnix = payment.TryGetProperty("created_at", out var createdProp) ? createdProp.GetInt64() : 0;
                var createdAt = DateTimeOffset.FromUnixTimeSeconds(createdAtUnix);

                // No FusionOS Payment-record wiring/domain event yet for a
                // synced Razorpay payment — see ConnectorSyncResult's doc
                // comment. id/amountPaise/currency/status/email/createdAt are
                // all successfully parsed above (proof the request+parse
                // genuinely works); intentionally not persisted this pass.
                _ = (id, amountPaise, currency, status, email, createdAt);
                processed++;
            }

            return new ConnectorSyncResult(true, processed, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return new ConnectorSyncResult(false, 0, $"Razorpay sync failed: {ex.Message}");
        }
    }
}
