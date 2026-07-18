using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.EnablePluginInstallation;

public sealed class EnablePluginInstallationCommandHandler : IRequestHandler<EnablePluginInstallationCommand, PluginInstallationDto>
{
    private readonly IPluginInstallationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public EnablePluginInstallationCommandHandler(IPluginInstallationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PluginInstallationDto> Handle(EnablePluginInstallationCommand request, CancellationToken cancellationToken)
    {
        var installation = await _repository.GetByIdAsync(request.CompanyId, request.PluginInstallationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin installation '{request.PluginInstallationId}' was not found.");

        installation.Enable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PluginInstallationMapper.ToDto(installation);
    }
}
