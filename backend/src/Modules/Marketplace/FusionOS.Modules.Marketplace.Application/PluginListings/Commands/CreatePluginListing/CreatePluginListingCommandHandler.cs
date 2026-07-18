using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Commands.CreatePluginListing;

public sealed class CreatePluginListingCommandHandler : IRequestHandler<CreatePluginListingCommand, PluginListingDto>
{
    private readonly IPluginListingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePluginListingCommandHandler(IPluginListingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PluginListingDto> Handle(CreatePluginListingCommand request, CancellationToken cancellationToken)
    {
        var listing = Domain.PluginListings.PluginListing.Create(request.CompanyId, request.Code, request.Name, request.Publisher, request.Category);

        await _repository.AddAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PluginListingMapper.ToDto(listing);
    }
}
