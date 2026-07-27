using System.Security.Cryptography;
using System.Text;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Commands.IngestShopifyOrderWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FusionOS.Modules.IntegrationHub.Api.Controllers;

/// <summary>
/// Phase 8 — AI &amp; Integrations. Inbound webhook receivers for connectors
/// that push events (as opposed to the poll-based sync adapters in
/// FusionOS.Modules.IntegrationHub.Infrastructure.Sync). Each action verifies
/// the provider's signature before doing anything with the payload — an
/// unverified/failed signature always returns 401, and the raw body is never
/// logged.
///
/// UPDATE (2026-07-25): <c>ConnectorConnection.CredentialReference</c> has now
/// landed. All three actions below resolve the connection's real
/// <c>CredentialReference</c> via <c>IConnectorConnectionRepository</c> (looked up by
/// <c>connectionId</c> alone — webhooks have no FusionOS company/user context, see
/// that repository's company-less <c>GetByIdAsync</c> overload) to name their webhook
/// secret's config key, the same way the sync adapters resolve theirs. The earlier
/// connectionId-as-credential-key placeholder Razorpay/Stripe briefly used has been
/// reconciled onto this same convention.
///
/// <b>[AllowAnonymous]:</b> Program.cs's FallbackPolicy requires an authenticated
/// caller on every endpoint by default (07_SECURITY.md's "wide-open by default"
/// closeout). Webhook senders (Shopify, Razorpay, Stripe) are external systems with
/// no FusionOS JWT to present — signature verification inside each action IS this
/// controller's auth boundary (a bad/missing signature returns 401 same as a failed
/// JWT would), the same reasoning CustomerPortalAuthController's login action uses
/// for its own [AllowAnonymous].
/// </summary>
[ApiController]
[Route("api/v1/integration-hub/webhooks")]
[AllowAnonymous]
public sealed class WebhooksController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhooksController> _logger;
    private readonly IConnectorConnectionRepository _connectionRepository;
    private readonly ISender _sender;

    public WebhooksController(
        IConfiguration configuration,
        ILogger<WebhooksController> logger,
        IConnectorConnectionRepository connectionRepository,
        ISender sender)
    {
        _configuration = configuration;
        _logger = logger;
        _connectionRepository = connectionRepository;
        _sender = sender;
    }

    /// <summary>
    /// Razorpay webhooks are signed with <c>X-Razorpay-Signature</c> =
    /// hex(HMAC-SHA256(webhook_secret, raw_body)) — hex encoding, not
    /// base64 (see https://razorpay.com/docs/webhooks/validate-test/).
    /// </summary>
    [HttpPost("razorpay/{connectionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Razorpay(Guid connectionId, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null || string.IsNullOrWhiteSpace(connection.CredentialReference))
        {
            _logger.LogWarning("Razorpay webhook rejected for connection {ConnectionId}: connection not found or no credential reference configured.", connectionId);
            return Unauthorized();
        }

        var webhookSecret = _configuration[$"{connection.CredentialReference}_WEBHOOK_SECRET"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            _logger.LogWarning("Razorpay webhook rejected for connection {ConnectionId}: no webhook secret configured.", connectionId);
            return Unauthorized();
        }

        var signatureHeader = Request.Headers["X-Razorpay-Signature"].ToString();
        if (string.IsNullOrEmpty(signatureHeader))
            return Unauthorized();

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var expectedSignature = ComputeHmacHex(webhookSecret, rawBody);
        if (!FixedTimeEquals(expectedSignature, signatureHeader))
        {
            _logger.LogWarning("Razorpay webhook signature verification failed for connection {ConnectionId}.", connectionId);
            return Unauthorized();
        }

        // Signature verified. Parsing/dispatching the specific event
        // (payment.captured, etc.) into a FusionOS domain event is deferred to
        // whatever shared "external event -> domain event" convention the
        // connector-sync framework settles on (see IConnectorSyncAdapter's doc
        // comment) — this pass proves signature verification works.
        return Ok();
    }

    /// <summary>
    /// Stripe webhooks are signed via the <c>Stripe-Signature</c> header,
    /// e.g. <c>t=1690000000,v1=abcdef...</c>. Verification recomputes
    /// hex(HMAC-SHA256(webhook_secret, "{t}.{raw_body}")) and compares it to
    /// the v1 value — NOT a plain HMAC of the raw body alone
    /// (see https://stripe.com/docs/webhooks#verify-manually).
    /// </summary>
    [HttpPost("stripe/{connectionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Stripe(Guid connectionId, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null || string.IsNullOrWhiteSpace(connection.CredentialReference))
        {
            _logger.LogWarning("Stripe webhook rejected for connection {ConnectionId}: connection not found or no credential reference configured.", connectionId);
            return Unauthorized();
        }

        var webhookSecret = _configuration[$"{connection.CredentialReference}_WEBHOOK_SECRET"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            _logger.LogWarning("Stripe webhook rejected for connection {ConnectionId}: no webhook secret configured.", connectionId);
            return Unauthorized();
        }

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrEmpty(signatureHeader))
            return Unauthorized();

        string? timestamp = null;
        string? v1Signature = null;
        foreach (var part in signatureHeader.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2)
                continue;

            if (kv[0].Trim() == "t")
                timestamp = kv[1].Trim();
            else if (kv[0].Trim() == "v1")
                v1Signature = kv[1].Trim();
        }

        if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(v1Signature))
            return Unauthorized();

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var signedPayload = $"{timestamp}.{rawBody}";
        var expectedSignature = ComputeHmacHex(webhookSecret, signedPayload);

        if (!FixedTimeEquals(expectedSignature, v1Signature))
        {
            _logger.LogWarning("Stripe webhook signature verification failed for connection {ConnectionId}.", connectionId);
            return Unauthorized();
        }

        // Signature verified — see the Razorpay action's equivalent comment
        // about event-to-domain-event wiring being deferred.
        return Ok();
    }

    /// <summary>
    /// Shopify signs webhooks with the <c>X-Shopify-Hmac-Sha256</c> header =
    /// base64(HMAC-SHA256(webhook_secret, raw_body)) — base64 encoding (unlike
    /// Razorpay's hex), computed over the exact raw request body bytes, no
    /// timestamp/prefix concatenation (unlike Stripe's <c>{t}.{body}</c> scheme). See
    /// https://shopify.dev/docs/apps/build/webhooks/subscribe/https#step-5-validate-the-webhook.
    ///
    /// The webhook secret is looked up as <c>{CredentialReference}_WEBHOOK_SECRET</c> —
    /// deliberately a SEPARATE config key from the plain <c>{CredentialReference}</c>
    /// key ShopifyConnectorSyncAdapter reads for the Admin API access token, since
    /// Shopify issues the webhook signing secret and the Admin API access token as two
    /// independent values, never one substituting for the other.
    /// </summary>
    [HttpPost("shopify/{connectionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Shopify(Guid connectionId, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null || string.IsNullOrWhiteSpace(connection.CredentialReference))
        {
            _logger.LogWarning("Shopify webhook rejected for connection {ConnectionId}: connection not found or no credential reference configured.", connectionId);
            return Unauthorized();
        }

        var webhookSecret = _configuration[$"{connection.CredentialReference}_WEBHOOK_SECRET"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            _logger.LogWarning("Shopify webhook rejected for connection {ConnectionId}: no webhook secret configured.", connectionId);
            return Unauthorized();
        }

        var hmacHeader = Request.Headers["X-Shopify-Hmac-Sha256"].ToString();
        if (string.IsNullOrEmpty(hmacHeader))
            return Unauthorized();

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        if (!VerifyShopifyHmac(webhookSecret, rawBody, hmacHeader))
        {
            _logger.LogWarning("Shopify webhook signature verification failed for connection {ConnectionId}.", connectionId);
            return Unauthorized();
        }

        // Signature verified — only now does the raw body get parsed/acted on.
        await _sender.Send(new IngestShopifyOrderWebhookCommand(connection.CompanyId, connectionId, rawBody), cancellationToken);
        return Ok();
    }

    /// <summary>base64(HMAC-SHA256(secret, rawBody)) compared to the header value in constant time. Decodes both sides from base64 to raw bytes before comparing — an invalid-base64 header fails closed (caught, returns false) rather than throwing out of a security check.</summary>
    private static bool VerifyShopifyHmac(string secret, string rawBody, string hmacHeaderBase64)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));

        byte[] headerBytes;
        try
        {
            headerBytes = Convert.FromBase64String(hmacHeaderBase64);
        }
        catch (FormatException)
        {
            return false;
        }

        if (computedHash.Length != headerBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(computedHash, headerBytes);
    }

    private static string ComputeHmacHex(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string expectedHex, string actualHex)
    {
        // CryptographicOperations.FixedTimeEquals requires equal-length spans;
        // an obviously-mismatched length is not itself timing-sensitive.
        if (expectedHex.Length != actualHex.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedHex),
            Encoding.UTF8.GetBytes(actualHex));
    }
}
