using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Commands.CreateFeatureFlag;

public sealed class CreateFeatureFlagCommandHandler : IRequestHandler<CreateFeatureFlagCommand, FeatureFlagDto>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFeatureFlagCommandHandler(IFeatureFlagRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FeatureFlagDto> Handle(CreateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.KeyExistsAsync(request.CompanyId, request.Key, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Key), $"A feature flag with key '{request.Key}' already exists for this company."),
            });
        }

        var flag = Domain.FeatureFlags.FeatureFlag.Create(request.CompanyId, request.Key, request.Name, request.Description, request.RolloutPercentage);

        await _repository.AddAsync(flag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FeatureFlagMapper.ToDto(flag);
    }
}
