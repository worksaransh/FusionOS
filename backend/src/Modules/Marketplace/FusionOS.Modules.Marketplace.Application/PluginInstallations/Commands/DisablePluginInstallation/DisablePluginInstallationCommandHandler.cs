using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.DisablePluginInstallation;

public sealed class DisablePluginInstallationCommandHandler : IRequestHandler<DisablePluginInstallationCommand, PluginInstallationDto>
{
    private readonly IPluginInstallationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DisablePluginInstallationCommandHandler(IPluginInstallationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PluginInstallationDto> Handle(DisablePluginInstallationCommand request, CancellationToken cancellationToken)
    {
        var installation = await _repository.GetByIdAsync(request.CompanyId, request.PluginInstallationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin installation '{request.PluginInstallationId}' was not found.");

        installation.Disable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PluginInstallationMapper.ToDto(installation);
    }
}
