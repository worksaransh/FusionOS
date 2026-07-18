using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Domain.CostCenters;
using MediatR;

namespace FusionOS.Modules.Finance.Application.CostCenters.Commands.CreateCostCenter;

public sealed class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, CostCenterDto>
{
    private readonly ICostCenterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCostCenterCommandHandler(ICostCenterRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CostCenterDto> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Cost center code '{request.Code}' already exists for this company."),
            });
        }

        var costCenter = CostCenter.Create(request.CompanyId, request.Code, request.Name);

        await _repository.AddAsync(costCenter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(costCenter);
    }

    internal static CostCenterDto MapToDto(CostCenter costCenter) => new(
        costCenter.Id, costCenter.Code, costCenter.Name, costCenter.IsActive, costCenter.CreatedAt);
}
