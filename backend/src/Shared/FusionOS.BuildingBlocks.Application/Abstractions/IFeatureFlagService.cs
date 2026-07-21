namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Cross-cutting feature-flag evaluation, published here (rather than in the Core
/// module) so any module can take a dependency on the abstraction without referencing
/// Core.Application directly — same shape as FusionOS.SharedKernel.Auditing.IAuditLogWriter,
/// which is likewise defined outside Core and implemented/registered by it. The concrete
/// implementation (FusionOS.Modules.Core.Application.FeatureFlags.Services.FeatureFlagService)
/// is a thin wrapper over IsFeatureEnabledQuery via ISender, registered once in
/// CoreModule.RegisterServices.
///
/// Caveat worth knowing before injecting this: because the implementation sends
/// IsFeatureEnabledQuery through the normal MediatR pipeline, a call still goes through
/// AuthorizationBehavior (the caller's ambient ICurrentUserContext must hold
/// "core.feature-flag.read") and TenantIsolationBehavior (companyId must match the
/// caller's own). That's fine for the common case — a module's handler checking a flag
/// while already servicing an authenticated HTTP request in that company — but it means
/// this is NOT safe to call from a background/system context with no signed-in user (no
/// ICurrentUserContext.CompanyId/permissions to check against), unlike e.g.
/// NotificationDeliveryService, which deliberately bypasses ISender entirely and talks to
/// its repository directly for exactly that reason. A future caller needing permission-less
/// internal flag checks (e.g. a Kafka consumer) should go through IFeatureFlagRepository
/// directly rather than this service, or this interface should grow a second
/// system-context implementation — deliberately not built now since nothing in this
/// codebase yet needs it (avoiding speculative scope).
/// </summary>
public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(Guid companyId, string key, string? evaluationId = null, CancellationToken cancellationToken = default);
}
