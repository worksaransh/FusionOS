using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Queries.GetPluginListingById;

public sealed class GetPluginListingByIdQueryHandler : IRequestHandler<GetPluginListingByIdQuery, PluginListingDto>
{
    private readonly IPluginListingRepository _repository;

    public GetPluginListingByIdQueryHandler(IPluginListingRepository repository) => _repository = repository;

    public async Task<PluginListingDto> Handle(GetPluginListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await _repository.GetByIdAsync(request.CompanyId, request.PluginListingId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin listing '{request.PluginListingId}' was not found.");

        return PluginListingMapper.ToDto(listing);
    }
}
