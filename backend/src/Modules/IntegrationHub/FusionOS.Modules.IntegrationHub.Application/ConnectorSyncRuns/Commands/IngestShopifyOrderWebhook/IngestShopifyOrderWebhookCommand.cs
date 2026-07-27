using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Commands.IngestShopifyOrderWebhook;

/// <summary>
/// Ingests one already-HMAC-verified Shopify order webhook payload
/// (WebhooksController verifies X-Shopify-Hmac-Sha256 before ever sending this
/// command). Deliberately NOT IRequirePermission: this is triggered by an external
/// system authenticated via HMAC signature, not a FusionOS user with a permission
/// set — the controller action is the security boundary here (401 on a bad/missing
/// signature), the same way CustomerPortalAuthController's login action is
/// [AllowAnonymous] rather than permission-gated.
/// Implements IPubliclyAccessible (Phase 10 audit fix, 2026-07-27): explicitly
/// exempts this HMAC-gated webhook command from AuthorizationBehavior's fail-closed check.
/// </summary>
public sealed record IngestShopifyOrderWebhookCommand(Guid CompanyId, Guid ConnectionId, string RawBody) : ICommand, IPubliclyAccessible;
