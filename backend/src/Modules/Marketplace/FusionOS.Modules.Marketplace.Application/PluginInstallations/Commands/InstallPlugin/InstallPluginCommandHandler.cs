using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.InstallPlugin;

/// <summary>Validates the PluginListing exists for this company before installing it — same handler-level existence-check split CreateJournalEntryCommandHandler uses for JournalEntryLine.AccountId.</summary>
public sealed class InstallPluginCommandHandler : IRequestHandler<InstallPluginCommand, PluginInstallationDto>
{
    private readonly IPluginInstallationRepository _repository;
    private readonly IPluginListingRepository _pluginListingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InstallPluginCommandHandler(IPluginInstallationRepository repository, IPluginListingRepository pluginListingRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _pluginListingRepository = pluginListingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PluginInstallationDto> Handle(InstallPluginCommand request, CancellationToken cancellationToken)
    {
        if (!await _pluginListingRepository.ExistsAsync(request.CompanyId, request.PluginListingId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.PluginListingId), $"Plugin listing '{request.PluginListingId}' does not exist for this company."),
            });
        }

        var installation = Domain.PluginInstallations.PluginInstallation.Create(request.CompanyId, request.PluginListingId);

        await _repository.AddAsync(installation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PluginInstallationMapper.ToDto(installation);
    }
}
