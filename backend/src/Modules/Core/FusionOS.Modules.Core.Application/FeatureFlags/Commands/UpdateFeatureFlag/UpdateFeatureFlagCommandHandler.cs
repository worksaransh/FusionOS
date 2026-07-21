using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Commands.UpdateFeatureFlag;

public sealed class UpdateFeatureFlagCommandHandler : IRequestHandler<UpdateFeatureFlagCommand, FeatureFlagDto>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFeatureFlagCommandHandler(IFeatureFlagRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FeatureFlagDto> Handle(UpdateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.CompanyId, request.FeatureFlagId, cancellationToken)
            ?? throw new KeyNotFoundException($"Feature flag '{request.FeatureFlagId}' was not found.");

        flag.UpdateDetails(request.Name, request.Description, request.RolloutPercentage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FeatureFlagMapper.ToDto(flag);
    }
}
