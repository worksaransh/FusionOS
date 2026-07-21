using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Commands.ToggleFeatureFlag;

public sealed class ToggleFeatureFlagCommandHandler : IRequestHandler<ToggleFeatureFlagCommand, FeatureFlagDto>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleFeatureFlagCommandHandler(IFeatureFlagRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FeatureFlagDto> Handle(ToggleFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.CompanyId, request.FeatureFlagId, cancellationToken)
            ?? throw new KeyNotFoundException($"Feature flag '{request.FeatureFlagId}' was not found.");

        flag.Toggle();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FeatureFlagMapper.ToDto(flag);
    }
}
