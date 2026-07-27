using System.Text.Json;
using FusionOS.Modules.IntegrationHub.Application.Sync;
using Microsoft.Extensions.Configuration;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Sync;

/// <summary>
/// Phase 8 — AI &amp; Integrations. Tracks a single shipment by waybill number
/// against Delhivery's real public tracking API
/// (https://track.delhivery.com/api/v1/packages/json/).
///
/// Auth: a static API token — <c>Authorization: Token {api_token}</c>, base
/// URL <c>https://track.delhivery.com/</c>. Config key:
/// <c>{credentialReference}_API_TOKEN</c>.
///
/// Unlike Razorpay/Stripe/Shiprocket, Delhivery's tracking API is not a
/// "list recent records" endpoint — it tracks ONE waybill number per call
/// (<c>?waybill=...</c>). There is no real "sync everything" action to call
/// here. A genuine production integration would iterate FusionOS's own
/// outbound shipment records (e.g. Sales' Dispatch aggregate) for their
/// carrier waybill numbers and call <see cref="TrackShipmentAsync"/> once per
/// waybill — but that requires a dispatch-to-waybill mapping this codebase
/// does not yet have (a prerequisite to check for in the Sales module before
/// wiring a real bulk sync). So <see cref="SyncAsync"/> is intentionally a
/// documented placeholder rather than a fabricated bulk sync; the real,
/// working per-shipment call is <see cref="TrackShipmentAsync"/>, callable
/// directly once that mapping exists.
/// </summary>
public sealed class DelhiveryConnectorSyncAdapter : IConnectorSyncAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public DelhiveryConnectorSyncAdapter(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri("https://track.delhivery.com/");
        _configuration = configuration;
    }

    public string ProviderName => "Delhivery";

    public Task<ConnectorSyncResult> SyncAsync(Guid companyId, string? credentialReference, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ConnectorSyncResult(false, 0,
            "Delhivery per-shipment tracking requires a waybill number per call — this adapter's SyncAsync is a placeholder; " +
            "use the same class's TrackShipmentAsync(waybillNumber) method directly once FusionOS has a real dispatch-to-waybill mapping to iterate."));
    }

    /// <summary>
    /// Tracks a single shipment. Returns the parsed shipment status records for
    /// the given waybill, or null if the credentials are not configured or the
    /// request failed. This is the reusable, genuinely-working call the class
    /// doc comment refers to — not wired into any command/handler yet.
    /// </summary>
    public async Task<IReadOnlyList<DelhiveryShipmentStatus>?> TrackShipmentAsync(string credentialReference, string waybillNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(credentialReference) || string.IsNullOrWhiteSpace(waybillNumber))
            return null;

        var apiToken = _configuration[$"{credentialReference}_API_TOKEN"];
        if (string.IsNullOrWhiteSpace(apiToken))
            return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/packages/json/?waybill={Uri.EscapeDataString(waybillNumber)}");
            request.Headers.TryAddWithoutValidation("Authorization", $"Token {apiToken}");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("ShipmentData", out var shipmentData) || shipmentData.ValueKind != JsonValueKind.Array)
                return Array.Empty<DelhiveryShipmentStatus>();

            var results = new List<DelhiveryShipmentStatus>();
            foreach (var entry in shipmentData.EnumerateArray())
            {
                if (!entry.TryGetProperty("Shipment", out var shipment))
                    continue;

                var awb = shipment.TryGetProperty("AWB", out var awbProp) ? awbProp.GetString() : waybillNumber;
                var status = shipment.TryGetProperty("Status", out var statusProp) && statusProp.TryGetProperty("Status", out var statusValue)
                    ? statusValue.GetString()
                    : null;

                results.Add(new DelhiveryShipmentStatus(awb ?? waybillNumber, status));
            }

            return results;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return null;
        }
    }
}

/// <summary>A single waybill's parsed tracking status from Delhivery.</summary>
public sealed record DelhiveryShipmentStatus(string WaybillNumber, string? Status);
