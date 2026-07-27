using System.Globalization;
using System.Text.Json;
using FusionOS.Modules.IntegrationHub.Application.Sync;
using Microsoft.Extensions.Configuration;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Sync;

/// <summary>
/// Phase 8 — AI &amp; Integrations. Syncs recent orders from Shopify's real public
/// REST Admin API (https://shopify.dev/docs/api/admin-rest/2024-01/resources/order).
///
/// Auth: Shopify's Admin API uses a single access token sent as the
/// <c>X-Shopify-Access-Token</c> header — no Basic Auth, no OAuth dance repeated per
/// call once a token exists. Unlike Razorpay (two secret halves), Shopify needs two
/// pieces of NON-secret-vs-secret config, both named off the same
/// <c>credentialReference</c>:
///   - <c>{credentialReference}</c> itself — the Shopify Admin API access token (secret).
///   - <c>{credentialReference}_SHOP_DOMAIN</c> — the store's *.myshopify.com domain
///     (e.g. "acme-store"), NOT a secret, but still config-driven since it varies per
///     connection and the request URL is templated on it
///     (<c>https://{shop}.myshopify.com/admin/api/2024-01/orders.json</c>).
/// Neither value is ever logged, persisted, or returned in a <see cref="ConnectorSyncResult"/> —
/// both are read from <see cref="IConfiguration"/> at call time only and held in a
/// local variable for the duration of the HTTP call.
///
/// This environment has no real Shopify store to test against, so the missing-credential
/// path (<see cref="IConfiguration"/> key absent) is the one path this adapter is
/// actually exercised through today — the request/parse path below is written to be
/// genuinely correct against Shopify's documented response shape for when a real store
/// is connected, not merely plausible-looking.
/// </summary>
public sealed class ShopifyConnectorSyncAdapter : IConnectorSyncAdapter
{
    private const string ApiVersion = "2024-01";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ShopifyConnectorSyncAdapter(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public string ProviderName => "Shopify";

    public async Task<ConnectorSyncResult> SyncAsync(Guid companyId, string? credentialReference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(credentialReference))
        {
            return new ConnectorSyncResult(false, 0,
                "Credential not configured — set the connection's credential reference first, then configure a " +
                "'{credentialReference}' environment variable/config key with a valid Shopify Admin API access token.");
        }

        var accessToken = _configuration[credentialReference];
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new ConnectorSyncResult(false, 0,
                $"Credential not configured — set the '{credentialReference}' environment variable/config key with a valid Shopify Admin API access token.");
        }

        var shopDomainKey = $"{credentialReference}_SHOP_DOMAIN";
        var shopDomain = _configuration[shopDomainKey];
        if (string.IsNullOrWhiteSpace(shopDomain))
        {
            return new ConnectorSyncResult(false, 0,
                $"Shop domain not configured — set the '{shopDomainKey}' environment variable/config key to the store's *.myshopify.com domain (e.g. 'acme-store').");
        }

        try
        {
            var requestUri = $"https://{shopDomain}.myshopify.com/admin/api/{ApiVersion}/orders.json";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("X-Shopify-Access-Token", accessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ConnectorSyncResult(false, 0,
                    $"Shopify API request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("orders", out var orders) || orders.ValueKind != JsonValueKind.Array)
                return new ConnectorSyncResult(true, 0, null);

            var syncedOrders = new List<SyncedOrderRecord>();
            foreach (var order in orders.EnumerateArray())
            {
                // Shopify order shape: id (number), email (string, may be null for
                // guest/no-email orders), total_price (decimal-as-string), created_at
                // (ISO-8601 string) — https://shopify.dev/docs/api/admin-rest/2024-01/resources/order#resource-object
                var id = order.TryGetProperty("id", out var idProp)
                    ? idProp.GetRawText()
                    : null;
                var email = order.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String
                    ? emailProp.GetString()
                    : null;
                var totalPriceRaw = order.TryGetProperty("total_price", out var totalProp) ? totalProp.GetString() : null;
                var createdAtRaw = order.TryGetProperty("created_at", out var createdProp) ? createdProp.GetString() : null;

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var totalAmount = decimal.TryParse(totalPriceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedTotal)
                    ? parsedTotal
                    : 0m;
                var orderedAt = DateTimeOffset.TryParse(createdAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                    ? parsedDate
                    : DateTimeOffset.UtcNow;

                syncedOrders.Add(new SyncedOrderRecord(id, email, totalAmount, orderedAt));
            }

            return new ConnectorSyncResult(true, syncedOrders.Count, null) { SyncedOrders = syncedOrders };
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return new ConnectorSyncResult(false, 0, $"Shopify sync failed: {ex.Message}");
        }
    }
}
