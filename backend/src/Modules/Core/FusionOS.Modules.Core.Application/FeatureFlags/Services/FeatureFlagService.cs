using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.FeatureFlags.Queries.IsFeatureEnabled;
using MediatR;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Services;

/// <summary>
/// Implements the cross-module IFeatureFlagService contract (BuildingBlocks.Application)
/// as a thin wrapper over IsFeatureEnabledQuery via ISender — see that interface's doc
/// comment for the full reasoning and its caveat about background/system-context callers.
/// </summary>
public sealed class FeatureFlagService : IFeatureFlagService
{
    private readonly ISender _sender;

    public FeatureFlagService(ISender sender) => _sender = sender;

    public Task<bool> IsEnabledAsync(Guid companyId, string key, string? evaluationId = null, CancellationToken cancellationToken = default) =>
        _sender.Send(new IsFeatureEnabledQuery(companyId, key, evaluationId), cancellationToken);
}
