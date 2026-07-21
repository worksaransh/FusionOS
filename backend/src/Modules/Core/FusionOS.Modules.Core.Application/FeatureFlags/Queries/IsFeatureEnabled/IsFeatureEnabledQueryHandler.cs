using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Queries.IsFeatureEnabled;

public sealed class IsFeatureEnabledQueryHandler : IRequestHandler<IsFeatureEnabledQuery, bool>
{
    private readonly IFeatureFlagRepository _repository;

    public IsFeatureEnabledQueryHandler(IFeatureFlagRepository repository) => _repository = repository;

    /// <summary>
    /// An unknown key resolves to false rather than throwing NotFound — a caller asking
    /// "is this flag on?" for a flag nobody has created yet is a normal, expected case
    /// (e.g. code that defensively checks a flag that ops hasn't rolled out to this
    /// company), not an error worth a 404/exception.
    /// </summary>
    public async Task<bool> Handle(IsFeatureEnabledQuery request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByKeyAsync(request.CompanyId, request.Key, cancellationToken);
        return flag is not null && flag.Evaluate(request.EvaluationId);
    }
}
