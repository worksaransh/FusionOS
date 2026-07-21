using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Queries.GetFeatureFlagById;

public sealed class GetFeatureFlagByIdQueryHandler : IRequestHandler<GetFeatureFlagByIdQuery, FeatureFlagDto>
{
    private readonly IFeatureFlagRepository _repository;

    public GetFeatureFlagByIdQueryHandler(IFeatureFlagRepository repository) => _repository = repository;

    public async Task<FeatureFlagDto> Handle(GetFeatureFlagByIdQuery request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.CompanyId, request.FeatureFlagId, cancellationToken)
            ?? throw new KeyNotFoundException($"Feature flag '{request.FeatureFlagId}' was not found.");

        return FeatureFlagMapper.ToDto(flag);
    }
}
