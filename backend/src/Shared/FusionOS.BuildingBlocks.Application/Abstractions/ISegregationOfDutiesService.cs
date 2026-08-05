namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Cross-cutting Segregation-of-Duties (SoD) conflict lookup, published here (rather than in
/// the Core module) so any module can take a dependency on the abstraction without
/// referencing Core.Application directly — same shape as IFeatureFlagService, whose doc
/// comment explains the pattern in full. The concrete implementation
/// (FusionOS.Modules.Core.Application.SodConflictRules.Services.SegregationOfDutiesService)
/// is registered once in CoreModule.RegisterServices.
///
/// Deliberately goes straight to the SodConflictRule repository rather than routing through
/// MediatR/ISender like IFeatureFlagService does: this is called from inside another
/// module's command handler (e.g. Procurement's ApprovePurchaseOrderCommandHandler) while
/// that handler's own permission check is already in flight, and the calling user will
/// typically not hold "core.sod-rule.read" — routing through the normal AuthorizationBehavior
/// pipeline would incorrectly reject the SoD check itself. See IFeatureFlagService's doc
/// comment for the equivalent caveat it flags but does not (yet) need to resolve.
/// </summary>
public interface ISegregationOfDutiesService
{
    /// <summary>
    /// True when an active SodConflictRule exists for this company covering both permission
    /// codes (in either order).
    /// </summary>
    Task<bool> HasConflictAsync(Guid companyId, string permissionA, string permissionB, CancellationToken cancellationToken = default);
}
