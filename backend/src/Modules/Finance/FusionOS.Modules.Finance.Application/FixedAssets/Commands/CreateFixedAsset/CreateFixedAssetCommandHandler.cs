using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;

/// <summary>
/// Validates the Code is unique for the company, that AssetAccountId (and,
/// if supplied, AccumulatedDepreciationAccountId and CostCenterId) all exist,
/// before creating the aggregate — same handler-level existence-check split
/// CreateBudgetLineCommandHandler uses for its own three references.
/// </summary>
public sealed class CreateFixedAssetCommandHandler : IRequestHandler<CreateFixedAssetCommand, FixedAssetDto>
{
    private readonly IFixedAssetRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFixedAssetCommandHandler(
        IFixedAssetRepository repository,
        IAccountRepository accountRepository,
        ICostCenterRepository costCenterRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FixedAssetDto> Handle(CreateFixedAssetCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Fixed asset code '{request.Code}' already exists for this company."),
            });
        }

        if (!await _accountRepository.ExistsAsync(request.CompanyId, request.AssetAccountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AssetAccountId), $"Account '{request.AssetAccountId}' does not exist for this company."),
            });
        }

        if (request.AccumulatedDepreciationAccountId.HasValue &&
            !await _accountRepository.ExistsAsync(request.CompanyId, request.AccumulatedDepreciationAccountId.Value, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AccumulatedDepreciationAccountId), $"Account '{request.AccumulatedDepreciationAccountId}' does not exist for this company."),
            });
        }

        if (request.CostCenterId.HasValue &&
            await _costCenterRepository.GetByIdAsync(request.CompanyId, request.CostCenterId.Value, cancellationToken) is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CostCenterId), $"Cost center '{request.CostCenterId}' does not exist for this company."),
            });
        }

        var fixedAsset = FixedAsset.Create(
            request.CompanyId, request.Code, request.Name, request.AssetAccountId,
            request.AccumulatedDepreciationAccountId, request.CostCenterId,
            request.AcquisitionDate, request.AcquisitionCost, request.SalvageValue, request.UsefulLifeMonths);

        await _repository.AddAsync(fixedAsset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(fixedAsset);
    }

    internal static FixedAssetDto MapToDto(FixedAsset fixedAsset) => new(
        fixedAsset.Id, fixedAsset.Code, fixedAsset.Name, fixedAsset.AssetAccountId,
        fixedAsset.AccumulatedDepreciationAccountId, fixedAsset.CostCenterId,
        fixedAsset.AcquisitionDate, fixedAsset.AcquisitionCost, fixedAsset.SalvageValue,
        fixedAsset.UsefulLifeMonths, fixedAsset.IsDisposed, fixedAsset.DisposedDate,
        fixedAsset.IsActive, fixedAsset.CreatedAt);
}
