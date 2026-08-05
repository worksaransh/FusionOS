using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.SodConflictRules.Contracts;

namespace FusionOS.Modules.Core.Application.SodConflictRules.Services;

/// <summary>
/// Implements the cross-module ISegregationOfDutiesService contract (BuildingBlocks.Application)
/// as a thin pass-through to ISodConflictRuleRepository — see that interface's doc comment for
/// why this deliberately bypasses ISender/MediatR (no permission requirement on the calling
/// context) unlike FeatureFlagService's equivalent wrapper.
/// </summary>
public sealed class SegregationOfDutiesService : ISegregationOfDutiesService
{
    private readonly ISodConflictRuleRepository _repository;

    public SegregationOfDutiesService(ISodConflictRuleRepository repository) => _repository = repository;

    public Task<bool> HasConflictAsync(Guid companyId, string permissionA, string permissionB, CancellationToken cancellationToken = default) =>
        _repository.HasActiveConflictAsync(companyId, permissionA, permissionB, cancellationToken);
}
