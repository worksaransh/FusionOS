using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Commands.UpdatePluginListing;

public sealed class UpdatePluginListingCommandHandler : IRequestHandler<UpdatePluginListingCommand, PluginListingDto>
{
    private readonly IPluginListingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePluginListingCommandHandler(IPluginListingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PluginListingDto> Handle(UpdatePluginListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _repository.GetByIdAsync(request.CompanyId, request.PluginListingId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin listing '{request.PluginListingId}' was not found.");

        listing.UpdateDetails(request.Name, request.Publisher, request.Category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PluginListingMapper.ToDto(listing);
    }
}
