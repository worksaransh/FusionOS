using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Commands.DeactivatePluginListing;

public sealed class DeactivatePluginListingCommandHandler : IRequestHandler<DeactivatePluginListingCommand, PluginListingDto>
{
    private readonly IPluginListingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePluginListingCommandHandler(IPluginListingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PluginListingDto> Handle(DeactivatePluginListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _repository.GetByIdAsync(request.CompanyId, request.PluginListingId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin listing '{request.PluginListingId}' was not found.");

        listing.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PluginListingMapper.ToDto(listing);
    }
}
