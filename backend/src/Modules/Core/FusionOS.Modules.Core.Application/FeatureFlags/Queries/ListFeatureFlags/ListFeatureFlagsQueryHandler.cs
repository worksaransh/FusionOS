using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Queries.ListFeatureFlags;

public sealed class ListFeatureFlagsQueryHandler : IRequestHandler<ListFeatureFlagsQuery, PagedResult<FeatureFlagDto>>
{
    private readonly IFeatureFlagRepository _repository;

    public ListFeatureFlagsQueryHandler(IFeatureFlagRepository repository) => _repository = repository;

    public async Task<PagedResult<FeatureFlagDto>> Handle(ListFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        var flags = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = flags.Select(FeatureFlagMapper.ToDto).ToList();

        return new PagedResult<FeatureFlagDto>(dtos, request.Page, request.PageSize, total);
    }
}
